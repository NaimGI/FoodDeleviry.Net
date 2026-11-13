using FoodOrdersApi.Models;

namespace FoodOrdersApi.Services;

public interface ICalorieEstimatorService
{
    CalorieEstimateDto Estimate(string name, string? description);
}

/// A transparent, deterministic food estimate. It is deliberately labelled as an estimate,
/// not a medical nutrition value; a nutrition provider can replace this service later.
public sealed class CalorieEstimatorService : ICalorieEstimatorService
{
    private static readonly (string Term, double Calories)[] Signals =
    {
        ("salad", 220), ("pizza", 850), ("burger", 700), ("pasta", 650),
        ("sandwich", 480), ("chicken", 420), ("rice", 520), ("fries", 420),
        ("dessert", 500), ("cake", 450), ("juice", 160), ("soup", 230)
    };

    public CalorieEstimateDto Estimate(string name, string? description)
    {
        var text = $"{name} {description}".ToLowerInvariant();
        var matches = Signals.Where(x => text.Contains(x.Term)).ToList();
        var calories = matches.Count == 0 ? 400 : matches.Average(x => x.Calories);
        var explanation = matches.Count == 0
            ? "Estimated from a typical restaurant-sized serving."
            : $"Estimated from a typical serving of {string.Join(", ", matches.Select(x => x.Term))}.";
        return new CalorieEstimateDto(Math.Round(calories), explanation);
    }
}
