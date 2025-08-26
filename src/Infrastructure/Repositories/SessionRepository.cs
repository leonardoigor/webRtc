using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Interfaces;

namespace WebRtcServer.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação em memória do repositório de sessões
    /// </summary>
    public class SessionRepository : ISessionRepository
    {
        private readonly ConcurrentDictionary<string, Session> _sessions = new();

        public Task<Session?> GetByIdAsync(string id)
        {
            _sessions.TryGetValue(id, out var session);
            return Task.FromResult(session);
        }

        public Task<Session?> GetActiveSessionByUserIdAsync(string userId)
        {
            var session = _sessions.Values.FirstOrDefault(s => s.UserId == userId && s.IsActive);
            return Task.FromResult(session);
        }

        public Task<IEnumerable<Session>> GetSessionsByUserIdAsync(string userId)
        {
            var sessions = _sessions.Values.Where(s => s.UserId == userId).ToList();
            return Task.FromResult<IEnumerable<Session>>(sessions);
        }

        public Task<IEnumerable<Session>> GetActiveSessionsAsync()
        {
            var sessions = _sessions.Values.Where(s => s.IsActive).ToList();
            return Task.FromResult<IEnumerable<Session>>(sessions);
        }

        public Task<IEnumerable<Session>> GetSessionsByStatusAsync(SessionStatus status)
        {
            var sessions = _sessions.Values.Where(s => s.Status == status).ToList();
            return Task.FromResult<IEnumerable<Session>>(sessions);
        }

        public Task<IEnumerable<Session>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sessions = _sessions.Values.Where(s => s.StartTime >= startDate && s.StartTime <= endDate).ToList();
            return Task.FromResult<IEnumerable<Session>>(sessions);
        }

        public Task<Session> AddAsync(Session session)
        {
            _sessions.TryAdd(session.Id, session);
            return Task.FromResult(session);
        }

        public Task<Session> UpdateAsync(Session session)
        {
            _sessions.TryUpdate(session.Id, session, _sessions[session.Id]);
            return Task.FromResult(session);
        }

        public Task DeleteAsync(string id)
        {
            _sessions.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string id)
        {
            return Task.FromResult(_sessions.ContainsKey(id));
        }

        public Task<int> GetActiveSessionsCountAsync()
        {
            var count = _sessions.Values.Count(s => s.IsActive);
            return Task.FromResult(count);
        }

        public Task<int> GetSharingSessionsCountAsync()
        {
            var count = _sessions.Values.Count(s => s.IsSharing);
            return Task.FromResult(count);
        }

        public Task<IEnumerable<Session>> GetSessionsWithRecordingsAsync()
        {
            var sessions = _sessions.Values.Where(s => s.Recordings.Any()).ToList();
            return Task.FromResult<IEnumerable<Session>>(sessions);
        }

        public Task<TimeSpan> GetTotalSessionTimeByUserIdAsync(string userId)
        {
            var totalTime = _sessions.Values
                .Where(s => s.UserId == userId && s.EndTime.HasValue)
                .Sum(s => s.GetDuration().TotalMilliseconds);
            
            return Task.FromResult(TimeSpan.FromMilliseconds(totalTime));
        }
    }
}