using TrainingCenter.Api.Models;

namespace TrainingCenter.Api.Services.Discounts;

/// <summary>
/// Предоставляет скидку 20%, если до старта осталось менее 120 часов
/// </summary>
public sealed class HotGroupDiscountRule : IDiscountRule
{
    /// <inheritdoc />
    public decimal GetDiscount(Course course, bool isGraduate, DateTimeOffset now)
    {
        var untilStart = course.StartsAt - now;
        return untilStart > TimeSpan.Zero && untilStart < TimeSpan.FromDays(5)
            ? 0.20m : 0m;
    }
}
