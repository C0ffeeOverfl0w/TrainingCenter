using System.ComponentModel.DataAnnotations;

namespace TrainingCenter.Api.Contracts;

/// <summary>
/// Тип клиента для выбора резервного адреса
/// </summary>
public enum ClientType
{
    /// <summary>
    /// Физическое лицо
    /// </summary>
    Individual = 1,
    
    /// <summary>
    /// Юридическое лицо
    /// </summary>
    Corporate = 2
}

/// <summary>
/// Заявка на обратный звонок
/// </summary>
public sealed class CreateLeadRequest
{
    /// <summary>
    /// Имя клиента
    /// </summary>
    [Required, StringLength(200)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Телефон
    /// </summary>
    [Required, StringLength(50)]
    public string Phone { get; init; } = string.Empty;

    /// <summary>
    /// Контактный email клиента
    /// </summary>
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Обязательный тип клиента
    /// </summary>
    [Required, EnumDataType(typeof(ClientType))]
    public ClientType? ClientType { get; init; }
}
