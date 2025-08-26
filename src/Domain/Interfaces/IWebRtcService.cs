using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.ValueObjects;

namespace WebRtcServer.Domain.Interfaces
{
    /// <summary>
    /// Interface para serviços WebRTC
    /// </summary>
    public interface IWebRtcService
    {
        Task<bool> CreateOfferAsync(string sessionId, string offer);
        Task<bool> CreateAnswerAsync(string sessionId, string answer);
        Task<bool> AddIceCandidateAsync(string sessionId, string candidate);
        Task<bool> StartStreamingAsync(string userId, string sessionId);
        Task<bool> StopStreamingAsync(string userId, string sessionId);
        Task<Connection> EstablishConnectionAsync(string sessionId, ConnectionId connectionId, string targetUserId);
        Task<bool> CloseConnectionAsync(ConnectionId connectionId);
        Task<bool> ValidateOfferAsync(string offer);
        Task<bool> ValidateAnswerAsync(string answer);
        Task<bool> ValidateIceCandidateAsync(string candidate);
    }
}