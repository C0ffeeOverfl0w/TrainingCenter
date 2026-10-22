namespace TrainingCenter.Api.Models;

/// <summary>
/// Курс выбранной учебной группы для расчёта корзины
/// </summary>
/// <param name="Name">Название курса</param>
/// <param name="Price">Цена до скидок</param>
/// <param name="StartsAt">Момент старта выбранной группы с часовым смещением</param>
public sealed record Course(string Name, decimal Price, DateTimeOffset StartsAt);
