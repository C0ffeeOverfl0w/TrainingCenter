using Microsoft.AspNetCore.Mvc;
using TrainingCenter.Api.Contracts;
using TrainingCenter.Api.Services;

namespace TrainingCenter.Api.Controllers;

/// <summary>
/// Принимает заявки и переключает отправку на резервный канал при отказе основного
/// </summary>
/// <param name="primarySender">Основной отправитель</param>
/// <param name="backupService">Сервис маршрутизации резервной отправки</param>
/// <param name="logger">Журнал технических событий без контактных данных клиента</param>
[ApiController]
[Route("api/leads")]
public sealed class LeadsController(IPrimaryEmailSender primarySender, BackupLeadService backupService,
    ILogger<LeadsController> logger) : ControllerBase
{
    /// <summary>
    /// Принимает заявку на обратный звонок
    /// </summary>
    /// <param name="request">Заявка, прошедшая валидацию</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса</param>
    /// <returns>Подтверждение после успешного вызова основного или резервного отправителя</returns>
    /// <response code="200">Заявка принята отправителем</response>
    /// <response code="400">Данные заявки некорректны</response>
    /// <response code="500">Заявку не удалось обработать, в том числе при отказе обоих каналов</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateLeadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await primarySender.SendAsync(request, cancellationToken);
        }
        catch (EmailDeliveryException exception)
        {
            logger.LogWarning(exception, "Primary email failed; using backup channel");
            await backupService.SendAsync(request, cancellationToken);
        }
        return Ok(new { message = "Заявка принята." });
    }
}
