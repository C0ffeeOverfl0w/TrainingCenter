using TrainingCenter.Api.Models;

namespace TrainingCenter.Api.Services.Discounts;

/// <summary>
/// Определяет доступную скидку для курса
/// </summary>
public interface IDiscountRule
{
    /// <summary>
    /// Вычисляет скидку
    /// </summary>
    /// <param name="course">Курс выбранной группы</param>
    /// <param name="isGraduate">Признак выпускника учебного центра</param>
    /// <param name="now">Единый момент времени для расчёта корзины</param>
    /// <returns>Доля от 0 до 1 включительно; 0, если правило неприменимо</returns>
    decimal GetDiscount(Course course, bool isGraduate, DateTimeOffset now);
}
