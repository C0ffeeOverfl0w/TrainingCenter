using TrainingCenter.Api.Models;

namespace TrainingCenter.Api.Services.Discounts;

/// <summary>
/// Предоставляет выпускникам скидку 30%
/// </summary>
public sealed class GraduateDiscountRule : IDiscountRule
{
    /// <inheritdoc />
    public decimal GetDiscount(Course course, bool isGraduate, DateTimeOffset now)
        => isGraduate ? 0.30m : 0m;
}
