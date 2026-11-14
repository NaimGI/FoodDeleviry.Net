using FoodOrdersApi.Entities;
using FoodOrdersApi.Entities.Objects;
using FoodOrdersApi.Models;
using FoodOrdersApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOrdersApi.Controllers;

[ApiController]
[Authorize]
public sealed class FeatureController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICalorieEstimatorService _calories;
    private readonly IOrderPricingService _pricing;

    public FeatureController(AppDbContext context, ICalorieEstimatorService calories, IOrderPricingService pricing)
        => (_context, _calories, _pricing) = (context, calories, pricing);

    private bool IsCurrentAccount(int accountId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentAccountId) && currentAccountId == accountId;

    private bool IsAdministrator() =>
        string.Equals(
            User.FindFirstValue(ClaimTypes.Name),
            "administrator",
            StringComparison.OrdinalIgnoreCase);

    [HttpGet("api/admin/reports")]
    public ActionResult GetAdminReports()
    {
        if (!IsAdministrator()) return Forbid();

        var reports = _context.Reports
            .OrderByDescending(report => report.CreatedAt)
            .Select(report => new
            {
                report.Id,
                Type = report.Type.ToString(),
                report.Description,
                Status = report.Status.ToString(),
                report.CreatedAt,
                report.OrderId,
                OrderUser = report.Order.Account.Login,
                report.MealId,
                MealName = report.Meal == null ? null : report.Meal.Name,
                report.RestaurantId,
                RestaurantName = report.Restaurant == null ? null : report.Restaurant.Name,
                report.AccountId
            })
            .ToList();

        return Ok(reports);
    }

    [HttpGet("api/admin/ratings")]
    public ActionResult GetAdminRatings()
    {
        if (!IsAdministrator()) return Forbid();

        var ratings = _context.OrderRatings
            .OrderByDescending(rating => rating.CreatedAt)
            .Select(rating => new
            {
                rating.Id,
                rating.OrderId,
                rating.Score,
                rating.Comment,
                rating.CreatedAt,
                OrderUser = rating.Order.Account.Login,
                TotalPrice = rating.Order.TotalPrice
            })
            .ToList();

        return Ok(ratings);
    }

    [HttpPost("api/calories/estimate")]
    public ActionResult<CalorieEstimateDto> EstimateCalories(CalorieEstimateRequest request) => Ok(_calories.Estimate(request.Name, request.Description));

    [HttpPost("api/pricing/quote")]
    public ActionResult<PricingQuoteDto> Quote(PricingQuoteRequest request)
    {
        if (!_context.Restaurants.Any(r => r.Id == request.RestaurantId)) return NotFound("Restaurant not found.");
        return Ok(_pricing.Quote(request));
    }

    [HttpPost("api/order/{orderId:int}/rating")]
    public ActionResult RateOrder(int orderId, CreateRatingDto request)
    {
        if (!IsCurrentAccount(request.AccountId)) return Forbid();
        var order = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.AccountId == request.AccountId);
        if (order == null) return NotFound("Order not found.");
        var rating = _context.OrderRatings.FirstOrDefault(r => r.OrderId == orderId);
        if (rating == null)
        {
            rating = new OrderRating { OrderId = orderId, Score = request.Score, Comment = request.Comment?.Trim(), CreatedAt = DateTime.UtcNow };
            _context.OrderRatings.Add(rating);
        }
        else
        {
            rating.Score = request.Score;
            rating.Comment = request.Comment?.Trim();
            rating.CreatedAt = DateTime.UtcNow;
        }
        _context.SaveChanges();
        return Ok();
    }

    [HttpPost("api/order/{orderId:int}/report")]
    public ActionResult ReportOrder(int orderId, CreateReportDto request)
    {
        if (!IsCurrentAccount(request.AccountId)) return Forbid();
        var order = _context.Orders.FirstOrDefault(o => o.Id == orderId && o.AccountId == request.AccountId);
        if (order == null) return NotFound("Order not found.");
        if (request.MealId.HasValue && !_context.MealOrder.Any(x => x.OrderId == orderId && x.MealId == request.MealId))
            return BadRequest("The selected meal is not part of this order.");

        var report = new Report
        {
            OrderId = orderId, AccountId = request.AccountId, MealId = request.MealId,
            RestaurantId = _context.Carts.Where(c => c.Id == order.CartId).Select(c => (int?)c.RestaurantId).FirstOrDefault(),
            Type = request.Type, Description = request.Description.Trim(), CreatedAt = DateTime.UtcNow
        };
        _context.Reports.Add(report);
        _context.SaveChanges();
        return Created($"api/report/{report.Id}", new { report.Id });
    }

    [HttpGet("api/notification/account/{accountId:int}")]
    public ActionResult<IEnumerable<NotificationDto>> Notifications(int accountId)
    {
        if (!IsCurrentAccount(accountId)) return Forbid();
        return Ok(_context.AppNotifications.Where(n => n.DstAccountId == accountId).OrderByDescending(n => n.Time)
            .Select(n => new NotificationDto(n.Id, n.Type.ToString(), n.Title, n.Message, n.Time, n.Read, n.OrderId)).ToList());
    }

    [HttpPatch("api/notification/{id:int}/read")]
    public ActionResult MarkRead(int id)
    {
        var notification = _context.AppNotifications.FirstOrDefault(n => n.Id == id);
        if (notification == null) return NotFound();
        if (!IsCurrentAccount(notification.DstAccountId)) return Forbid();
        notification.Read = true;
        _context.SaveChanges();
        return NoContent();
    }
}
