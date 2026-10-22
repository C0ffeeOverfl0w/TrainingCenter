using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using TrainingCenter.Api.Contracts;

namespace TrainingCenter.Api.Services;

/// <summary>
/// Резервные адреса из секции конфигурации Email
/// </summary>
public sealed class EmailOptions
{
    /// <summary>
    /// Адрес для заявок юридических лиц
    /// </summary>
    [Required, EmailAddress]
    public string CorporateAddress { get; init; } = string.Empty;
    /// <summary>
    /// Адрес для заявок физических лиц
    /// </summary>
    [Required, EmailAddress]
    public string IndividualAddress { get; init; } = string.Empty;
}

/// <summary>
/// Ожидаемый отказ почтового канала
/// </summary>
/// <param name="message">
/// Техническое описание отказа без персональных данных
/// </param>
public sealed class EmailDeliveryException(string message) : Exception(message);

/// <summary>
/// Отправляет заявку через основной почтовый канал
/// </summary>
public interface IPrimaryEmailSender
{
    /// <summary>
    /// Передаёт заявку основному каналу
    /// </summary>
    /// <param name="lead">Заявка, прошедшая валидацию</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Завершение передачи заявки</returns>
    /// <exception cref="EmailDeliveryException">Основной канал недоступен</exception>
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    Task SendAsync(CreateLeadRequest lead, CancellationToken cancellationToken);
}

/// <summary>
/// Отправляет заявку через резервный канал указанному получателю
/// </summary>
public interface IBackupEmailSender
{
    /// <summary>
    /// Передаёт заявку резервному каналу
    /// </summary>
    /// <param name="recipient">Email специалиста, выбранный по типу клиента</param>
    /// <param name="lead">Заявка, прошедшая валидацию</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Завершение передачи заявки</returns>
    /// <exception cref="EmailDeliveryException">Резервный канал не смог принять заявку</exception>
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    Task SendAsync(string recipient, CreateLeadRequest lead, CancellationToken cancellationToken);
}

/// <summary>
/// Имитирует постоянный отказ основного канала
/// </summary>
public sealed class UnavailablePrimaryEmailSender : IPrimaryEmailSender
{
    /// <inheritdoc />
    public Task SendAsync(CreateLeadRequest lead, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromException(new EmailDeliveryException("Primary email service is unavailable."));
    }
}

/// <summary>
/// Имитирует успешную резервную отправку
/// </summary>
/// <param name="logger">Журнал технических событий; 
/// контактные данные клиента не записываются
/// </param>
public sealed class SimulatedBackupEmailSender(ILogger<SimulatedBackupEmailSender> logger) : IBackupEmailSender
{
    /// <inheritdoc />
    public Task SendAsync(string recipient, CreateLeadRequest lead, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Simulated backup email accepted for {Recipient}", recipient);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Выбирает резервный адрес по типу клиента и передаёт заявку отправителю
/// </summary>
/// <param name="sender">Резервный отправитель</param>
/// <param name="options">Адреса</param>
public sealed class BackupLeadService(IBackupEmailSender sender, IOptions<EmailOptions> options)
{
    /// <summary>
    /// Направляет заявку специалисту
    /// </summary>
    /// <param name="lead">Заявка с допустимым типом клиента</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Задача завершения резервной отправки.</returns>
    /// <exception cref="ArgumentException">Тип клиента не указан или не поддерживается</exception>
    /// <exception cref="EmailDeliveryException">Резервный канал не смог принять заявку</exception>
    /// <exception cref="OperationCanceledException">Операция отменена</exception>
    public Task SendAsync(CreateLeadRequest lead, CancellationToken cancellationToken)
    {
        var recipient = lead.ClientType switch
        {
            ClientType.Corporate => options.Value.CorporateAddress,
            ClientType.Individual => options.Value.IndividualAddress,
            _ => throw new ArgumentException("Unsupported client type.", nameof(lead))
        };
        return sender.SendAsync(recipient, lead, cancellationToken);
    }
}
