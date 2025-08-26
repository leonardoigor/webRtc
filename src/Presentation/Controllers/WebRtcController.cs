using Microsoft.AspNetCore.Mvc;
using WebRtcServer.Application.Interfaces;
using WebRtcServer.Domain.Interfaces;
using WebRtcServer.Domain.ValueObjects;

namespace WebRtcServer.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebRtcController : ControllerBase
{
    private readonly IWebRtcService _webRtcService;

    public WebRtcController(IWebRtcService webRtcService)
    {
        _webRtcService = webRtcService;
    }

    /// <summary>
    /// Cria uma oferta WebRTC
    /// </summary>
    /// <param name="request">Dados da oferta</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("offer")]
    public async Task<ActionResult> CreateOffer([FromBody] CreateOfferRequest request)
    {
        try
        {
            await _webRtcService.CreateOfferAsync(request.ConnectionId, request.Offer);
            return Ok(new { message = "Oferta criada com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cria uma resposta WebRTC
    /// </summary>
    /// <param name="request">Dados da resposta</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("answer")]
    public async Task<ActionResult> CreateAnswer([FromBody] CreateAnswerRequest request)
    {
        try
        {
            await _webRtcService.CreateAnswerAsync(request.ConnectionId, request.Answer);
            return Ok(new { message = "Resposta criada com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adiciona um candidato ICE
    /// </summary>
    /// <param name="request">Dados do candidato ICE</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("ice-candidate")]
    public async Task<ActionResult> AddIceCandidate([FromBody] AddIceCandidateRequest request)
    {
        try
        {
            await _webRtcService.AddIceCandidateAsync(request.ConnectionId, request.Candidate);
            return Ok(new { message = "Candidato ICE adicionado com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Inicia o streaming
    /// </summary>
    /// <param name="request">Dados para iniciar o streaming</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("start-streaming")]
    public async Task<ActionResult> StartStreaming([FromBody] StartStreamingRequest request)
    {
        try
        {
            await _webRtcService.StartStreamingAsync(request.UserId, request.SessionId);
            return Ok(new { message = "Streaming iniciado com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Para o streaming
    /// </summary>
    /// <param name="request">Dados para parar o streaming</param>
    /// <returns>Resultado da operação</returns>
    [HttpPost("stop-streaming")]
    public async Task<ActionResult> StopStreaming([FromBody] StopStreamingRequest request)
    {
        try
        {
            await _webRtcService.StopStreamingAsync(request.UserId, request.SessionId);
            return Ok(new { message = "Streaming parado com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Fecha uma conexão WebRTC
    /// </summary>
    /// <param name="connectionId">ID da conexão</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("connections/{connectionId}")]
    public async Task<ActionResult> CloseConnection(string connectionId)
    {
        try
        {
            await _webRtcService.CloseConnectionAsync(new ConnectionId(connectionId));
            return Ok(new { message = "Conexão fechada com sucesso" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// DTOs para as requisições
public class CreateOfferRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Offer { get; set; } = string.Empty;
}

public class CreateAnswerRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class AddIceCandidateRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Candidate { get; set; } = string.Empty;
}

public class StartStreamingRequest
{
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

public class StopStreamingRequest
{
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}