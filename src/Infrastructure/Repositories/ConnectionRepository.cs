using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Interfaces;
using WebRtcServer.Domain.ValueObjects;

namespace WebRtcServer.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação em memória do repositório de conexões
    /// </summary>
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly ConcurrentDictionary<string, Connection> _connections = new();

        public Task<Connection?> GetByIdAsync(string id)
        {
            _connections.TryGetValue(id, out var connection);
            return Task.FromResult(connection);
        }

        public Task<Connection?> GetByConnectionIdAsync(ConnectionId connectionId)
        {
            var connection = _connections.Values.FirstOrDefault(c => c.ConnectionId == connectionId);
            return Task.FromResult(connection);
        }

        public Task<IEnumerable<Connection>> GetBySessionIdAsync(string sessionId)
        {
            var connections = _connections.Values.Where(c => c.SessionId == sessionId).ToList();
            return Task.FromResult<IEnumerable<Connection>>(connections);
        }

        public Task<IEnumerable<Connection>> GetActiveConnectionsAsync()
        {
            var connections = _connections.Values.Where(c => c.IsActive).ToList();
            return Task.FromResult<IEnumerable<Connection>>(connections);
        }

        public Task<IEnumerable<Connection>> GetConnectionsByStatusAsync(ConnectionStatus status)
        {
            var connections = _connections.Values.Where(c => c.Status == status).ToList();
            return Task.FromResult<IEnumerable<Connection>>(connections);
        }

        public Task<IEnumerable<Connection>> GetConnectionsByTypeAsync(ConnectionType type)
        {
            var connections = _connections.Values.Where(c => c.Type == type).ToList();
            return Task.FromResult<IEnumerable<Connection>>(connections);
        }

        public Task<IEnumerable<Connection>> GetConnectionsByTargetUserIdAsync(string targetUserId)
        {
            var connections = _connections.Values.Where(c => c.TargetUserId == targetUserId).ToList();
            return Task.FromResult<IEnumerable<Connection>>(connections);
        }

        public Task<Connection> AddAsync(Connection connection)
        {
            _connections.TryAdd(connection.Id, connection);
            return Task.FromResult(connection);
        }

        public Task<Connection> UpdateAsync(Connection connection)
        {
            _connections.TryUpdate(connection.Id, connection, _connections[connection.Id]);
            return Task.FromResult(connection);
        }

        public Task DeleteAsync(string id)
        {
            _connections.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string id)
        {
            return Task.FromResult(_connections.ContainsKey(id));
        }

        public Task<bool> ExistsByConnectionIdAsync(ConnectionId connectionId)
        {
            var exists = _connections.Values.Any(c => c.ConnectionId == connectionId);
            return Task.FromResult(exists);
        }

        public Task<int> GetActiveConnectionsCountAsync()
        {
            var count = _connections.Values.Count(c => c.IsActive);
            return Task.FromResult(count);
        }

        public Task<int> GetConnectionsCountBySessionIdAsync(string sessionId)
        {
            var count = _connections.Values.Count(c => c.SessionId == sessionId);
            return Task.FromResult(count);
        }

        public Task<TimeSpan> GetAverageConnectionDurationAsync()
        {
            var closedConnections = _connections.Values.Where(c => c.ClosedAt.HasValue).ToList();
            
            if (!closedConnections.Any())
                return Task.FromResult(TimeSpan.Zero);

            var averageTicks = closedConnections.Average(c => c.GetDuration().Ticks);
            return Task.FromResult(new TimeSpan((long)averageTicks));
        }

        public Task<IEnumerable<Connection>> GetConnectionsWithErrorsAsync()
        {
            var connections = _connections.Values.Where(c => c.Status == ConnectionStatus.Error).ToList();
            return Task.FromResult<IEnumerable<Connection>>(connections);
        }
    }
}