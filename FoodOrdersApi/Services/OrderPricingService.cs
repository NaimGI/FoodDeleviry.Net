using FoodOrdersApi.Entities;
using FoodOrdersApi.Entities.Objects;
using FoodOrdersApi.Models;

namespace FoodOrdersApi.Services;

public interface IOrderPricingService
{
    PricingQuoteDto Quote(PricingQuoteRequest request);
    void Recalculate(Order order);
}

public sealed class OrderPricingService : IOrderPricingService
{
    private const double DefaultDeliveryFee = 3.0;
    private const double BadWeatherSurcharge = 2.0;
    private const double PrioritySurcharge = 2.5;
    private const double TaxRate = 0.07;
    private readonly AppDbContext _context;

    public OrderPricingService(AppDbContext context) => _context = context;

    public PricingQuoteDto Quote(PricingQuoteRequest request)
    {
        var configured = _context.DeliveryPricings
            .Where(x => x.RestaurantId == null || x.RestaurantId == request.RestaurantId)
            .ToList();
        var baseFee = configured.FirstOrDefault(x => x.Category == 0)?.Value ?? DefaultDeliveryFee;
        var weatherFee = request.IsBadWeather ? configured.FirstOrDefault(x => x.Category == 1)?.Value ?? BadWeatherSurcharge : 0;
        var priorityFee = request.IsPriority ? configured.FirstOrDefault(x => x.Category == 2)?.Value ?? PrioritySurcharge : 0;
        var delivery = Math.Round(baseFee + weatherFee + priorityFee, 2);
        var tax = Math.Round(request.Subtotal * TaxRate, 2);
        return new PricingQuoteDto(request.Subtotal, delivery, tax, Math.Round(request.Subtotal + delivery + tax, 2), request.IsPriority, request.IsBadWeather);
    }

    public void Recalculate(Order order)
    {
        var quote = Quote(new PricingQuoteRequest
        {
            RestaurantId = order.Cart.RestaurantId,
            Subtotal = order.Subtotal,
            IsPriority = order.IsPriority,
            IsBadWeather = order.IsBadWeather
        });
        order.DeliveryFee = quote.DeliveryFee;
        order.Tax = quote.Tax;
        order.TotalPrice = quote.Total;
    }
}
