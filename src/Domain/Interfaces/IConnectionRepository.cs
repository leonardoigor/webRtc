using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.ValueObjects;

namespace WebRtcServer.Domain.Interfaces
{
    /// <summary>
    /// Interface para repositório de conexões
    /// </summary>
    public interface IConnectionRepository
    {
        Task<Connection?> GetByIdAsync(string id);
        Task<Connection?> GetByConnectionIdAsync(ConnectionId connectionId);
        Task<IEnumerable<Connection>> GetBySessionIdAsync(string sessionId);
        Task<IEnumerable<Connection>> GetActiveConnectionsAsync();
        Task<IEnumerable<Connection>> GetConnectionsByStatusAsync(ConnectionStatus status);
        Task<IEnumerable<Connection>> GetConnectionsByTypeAsync(ConnectionType type);
        Task<IEnumerable<Connection>> GetConnectionsByTargetUserIdAsync(string targetUserId);
        Task<Connection> AddAsync(Connection connection);
        Task<Connection> UpdateAsync(Connection connection);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> ExistsByConnectionIdAsync(ConnectionId connectionId);
        Task<int> GetActiveConnectionsCountAsync();
        Task<int> GetConnectionsCountBySessionIdAsync(string sessionId);
        Task<TimeSpan> GetAverageConnectionDurationAsync();
        Task<IEnumerable<Connection>> GetConnectionsWithErrorsAsync();
    }
}