using Microsoft.AspNetCore.Mvc;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Application.Interfaces;

namespace WebRtcServer.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private readonly IStreamingService _streamingService;

    public StreamingController(IStreamingService streamingService)
    {
        _streamingService = streamingService;
    }

    /// <summary>
    /// Inicia uma nova sessão de streaming
    /// </summary>
    /// <param name="userId">ID do usuário que iniciará a sessão</param>
    /// <returns>Dados da sessão criada</returns>
    [HttpPost("sessions")]
    public async Task<ActionResult<SessionDto>> StartSession([FromBody] string userId)
    {
        try
        {
            var sessionDto = await _streamingService.StartSessionAsync(userId);
            return CreatedAtAction(nameof(GetSession), new { sessionId = sessionDto.Id }, sessionDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém uma sessão pelo ID
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <returns>Dados da sessão</returns>
    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<SessionDto>> GetSession(string sessionId)
    {
        try
        {
            var session = await _streamingService.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return NotFound(new { message = "Sessão não encontrada" });
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todas as sessões de um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Lista de sessões do usuário</returns>
    [HttpGet("users/{userId}/sessions")]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetUserSessions(string userId)
    {
        try
        {
            var sessions = await _streamingService.GetSessionsByUserIdAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Inicia o compartilhamento de uma sessão
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("sessions/{sessionId}/start-sharing")]
    public async Task<ActionResult> StartSharing(string sessionId)
    {
        try
        {
            await _streamingService.StartSharingAsync(sessionId);
            return Ok(new { message = "Compartilhamento iniciado" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Para o compartilhamento de uma sessão
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("sessions/{sessionId}/stop-sharing")]
    public async Task<ActionResult> StopSharing(string sessionId)
    {
        try
        {
            await _streamingService.StopSharingAsync(sessionId);
            return Ok(new { message = "Compartilhamento parado" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Finaliza uma sessão
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("sessions/{sessionId}")]
    public async Task<ActionResult> EndSession(string sessionId)
    {
        try
        {
            await _streamingService.EndSessionAsync(sessionId);
            return Ok(new { message = "Sessão finalizada" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cria uma nova conexão em uma sessão
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <param name="createConnectionDto">Dados da conexão a ser criada</param>
    /// <returns>Dados da conexão criada</returns>
    [HttpPost("sessions/{sessionId}/connections")]
    public async Task<ActionResult<ConnectionDto>> CreateConnection(string sessionId, [FromBody] CreateConnectionDto createConnectionDto)
    {
        try
        {
            var connectionDto = await _streamingService.CreateConnectionAsync(createConnectionDto);
            return CreatedAtAction(nameof(GetSessionConnections), new { sessionId = sessionId }, connectionDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Fecha uma conexão
    /// </summary>
    /// <param name="connectionId">ID da conexão</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("connections/{connectionId}")]
    public async Task<ActionResult> CloseConnection(string connectionId)
    {
        try
        {
            await _streamingService.CloseConnectionAsync(connectionId);
            return Ok(new { message = "Conexão fechada" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém conexões de uma sessão
    /// </summary>
    /// <param name="sessionId">ID da sessão</param>
    /// <returns>Lista de conexões da sessão</returns>
    [HttpGet("sessions/{sessionId}/connections")]
    public async Task<ActionResult<IEnumerable<ConnectionDto>>> GetSessionConnections(string sessionId)
    {
        try
        {
            var connections = await _streamingService.GetConnectionsBySessionIdAsync(sessionId);
            return Ok(connections);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todas as conexões ativas
    /// </summary>
    /// <returns>Lista de conexões ativas</returns>
    [HttpGet("connections/active")]
    public async Task<ActionResult<IEnumerable<ConnectionDto>>> GetActiveConnections()
    {
        try
        {
            var connections = await _streamingService.GetActiveConnectionsAsync();
            return Ok(connections);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}