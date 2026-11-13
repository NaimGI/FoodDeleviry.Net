using System.ComponentModel.DataAnnotations;
using FoodOrdersApi.Entities.Enum.Report;

namespace FoodOrdersApi.Models;

public sealed class CalorieEstimateRequest
{
    [Required] public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public sealed record CalorieEstimateDto(double Calories, string Explanation, string Source = "Estimated");

public sealed class PricingQuoteRequest
{
    [Range(1, int.MaxValue)] public int RestaurantId { get; set; }
    [Range(0, double.MaxValue)] public double Subtotal { get; set; }
    public bool IsPriority { get; set; }
    public bool IsBadWeather { get; set; }
}

public sealed record PricingQuoteDto(double Subtotal, double DeliveryFee, double Tax, double Total, bool IsPriority, bool IsBadWeather);

public sealed class CreateRatingDto
{
    [Range(1, int.MaxValue)] public int AccountId { get; set; }
    [Range(1, 5)] public int Score { get; set; }
    [StringLength(500)] public string? Comment { get; set; }
}

public sealed class CreateReportDto
{
    [Range(1, int.MaxValue)] public int AccountId { get; set; }
    public ReportType Type { get; set; }
    [Required, StringLength(1000)] public string Description { get; set; } = "";
    public int? MealId { get; set; }
}

public sealed record NotificationDto(int Id, string Type, string Title, string Message, DateTime Time, bool Read, int? OrderId);
