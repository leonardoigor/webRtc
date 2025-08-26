using Microsoft.AspNetCore.Mvc;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Application.Interfaces;

namespace WebRtcServer.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecordingController : ControllerBase
{
    private readonly IRecordingApplicationService _recordingService;

    public RecordingController(IRecordingApplicationService recordingService)
    {
        _recordingService = recordingService;
    }

    /// <summary>
    /// Inicia uma nova gravação
    /// </summary>
    /// <param name="startRecordingDto">Dados para iniciar a gravação</param>
    /// <returns>Dados da gravação criada</returns>
    [HttpPost]
    public async Task<ActionResult<RecordingDto>> StartRecording([FromBody] StartRecordingDto startRecordingDto)
    {
        try
        {
            var recordingDto = await _recordingService.StartRecordingAsync(startRecordingDto);
            return CreatedAtAction(nameof(GetRecording), new { recordingId = recordingDto.Id }, recordingDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém uma gravação pelo ID
    /// </summary>
    /// <param name="recordingId">ID da gravação</param>
    /// <returns>Dados da gravação</returns>
    [HttpGet("{recordingId}")]
    public async Task<ActionResult<RecordingDto>> GetRecording(string recordingId)
    {
        try
        {
            var recording = await _recordingService.GetRecordingByIdAsync(recordingId);
            if (recording == null)
            {
                return NotFound(new { message = "Gravação não encontrada" });
            }
            return Ok(recording);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todas as gravações de uma sessão
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <returns>Lista de gravações da sessão</returns>
    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<IEnumerable<RecordingDto>>> GetSessionRecordings(string sessionId)
    {
        try
        {
            var recordings = await _recordingService.GetRecordingsBySessionIdAsync(sessionId);
            return Ok(recordings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Para uma gravação
    /// </summary>
    /// <param name="recordingId">ID da gravação</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{recordingId}/stop")]
    public async Task<ActionResult> StopRecording(string recordingId)
    {
        try
        {
            await _recordingService.StopRecordingAsync(recordingId);
            return Ok(new { message = "Gravação parada" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Pausa uma gravação
    /// </summary>
    /// <param name="recordingId">ID da gravação</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{recordingId}/pause")]
    public async Task<ActionResult> PauseRecording(string recordingId)
    {
        try
        {
            await _recordingService.PauseRecordingAsync(recordingId);
            return Ok(new { message = "Gravação pausada" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retoma uma gravação pausada
    /// </summary>
    /// <param name="recordingId">ID da gravação</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{recordingId}/resume")]
    public async Task<ActionResult> ResumeRecording(string recordingId)
    {
        try
        {
            await _recordingService.ResumeRecordingAsync(recordingId);
            return Ok(new { message = "Gravação retomada" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Exclui uma gravação
    /// </summary>
    /// <param name="recordingId">ID da gravação</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{recordingId}")]
    public async Task<ActionResult> DeleteRecording(string recordingId)
    {
        try
        {
            await _recordingService.DeleteRecordingAsync(recordingId);
            return Ok(new { message = "Gravação excluída" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Faz download de uma gravação
    /// </summary>
    /// <param name="recordingId">ID da gravação</param>
    /// <returns>Arquivo da gravação</returns>
    [HttpGet("{recordingId}/download")]
    public async Task<ActionResult> DownloadRecording(string recordingId)
    {
        try
        {
            var recording = await _recordingService.GetRecordingByIdAsync(recordingId);
            if (recording == null)
            {
                return NotFound(new { message = "Gravação não encontrada" });
            }

            if (!System.IO.File.Exists(recording.FilePath))
            {
                return NotFound(new { message = "Arquivo de gravação não encontrado" });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(recording.FilePath);
            var fileName = System.IO.Path.GetFileName(recording.FilePath);
            
            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}