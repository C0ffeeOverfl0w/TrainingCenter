using TrainingCenter.Api.Models;
using TrainingCenter.Api.Services.Discounts;

namespace TrainingCenter.Api.Services;

/// <summary>
/// Рассчитывает корзину с максимальной скидкой для каждого курса
/// </summary>
/// <param name="timeProvider">Источник времени</param>
/// <param name="rules">Независимые правила</param>
public sealed class CartPricingService(TimeProvider timeProvider, IEnumerable<IDiscountRule> rules)
{
    private readonly IDiscountRule[] discountRules = [.. rules];

    /// <summary>
    /// Применяет одну максимальную скидку к каждому курсу и суммирует стоимость
    /// </summary>
    /// <param name="courses">Курсы выбранных групп</param>
    /// <param name="isGraduate">Признак выпускника учебного центра</param>
    /// <returns>Итоговая сумма; для пустой корзины - 0</returns>
    /// <exception cref="ArgumentNullException">Коллекция или один из элементов равны null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Цена курса отрицательна</exception>
    public decimal CalculateTotal(IEnumerable<Course> courses, bool isGraduate)
    {
        ArgumentNullException.ThrowIfNull(courses);
        var now = timeProvider.GetUtcNow();
        decimal total = 0;
        foreach (var course in courses)
        {
            ArgumentNullException.ThrowIfNull(course);
            ArgumentOutOfRangeException.ThrowIfNegative(course.Price);
            var discount = discountRules
                .Select(rule => rule.GetDiscount(course, isGraduate, now))
                .DefaultIfEmpty(0m)
                .Max();
            total += decimal.Round(course.Price * (1m - discount), 2, MidpointRounding.AwayFromZero);
        }
        return total;
    }
}
