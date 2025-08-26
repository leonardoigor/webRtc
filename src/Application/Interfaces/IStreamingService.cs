using System.Collections.Generic;
using System.Threading.Tasks;
using WebRtcServer.Application.DTOs;

namespace WebRtcServer.Application.Interfaces
{
    /// <summary>
    /// Interface para serviços de streaming na camada de aplicação
    /// </summary>
    public interface IStreamingService
    {
        Task<SessionDto> StartSessionAsync(string userId);
        Task<bool> EndSessionAsync(string sessionId);
        Task<bool> StartSharingAsync(string sessionId);
        Task<bool> StopSharingAsync(string sessionId);
        Task<SessionDto?> GetSessionByIdAsync(string sessionId);
        Task<SessionDto?> GetActiveSessionByUserIdAsync(string userId);
        Task<IEnumerable<SessionDto>> GetSessionsByUserIdAsync(string userId);
        Task<IEnumerable<SessionDto>> GetActiveSessionsAsync();
        Task<ConnectionDto> CreateConnectionAsync(CreateConnectionDto createConnectionDto);
        Task<bool> CloseConnectionAsync(string connectionId);
        Task<IEnumerable<ConnectionDto>> GetConnectionsBySessionIdAsync(string sessionId);
        Task<IEnumerable<ConnectionDto>> GetActiveConnectionsAsync();
        Task<bool> HandleWebRtcOfferAsync(string sessionId, string offer);
        Task<bool> HandleWebRtcAnswerAsync(string sessionId, string answer);
        Task<bool> HandleIceCandidateAsync(string sessionId, string candidate);
        Task<int> GetActiveSessionsCountAsync();
        Task<int> GetSharingSessionsCountAsync();
    }
}