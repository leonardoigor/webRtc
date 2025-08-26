using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;

namespace WebRtcServer.Domain.Interfaces
{
    /// <summary>
    /// Interface para repositório de sessões
    /// </summary>
    public interface ISessionRepository
    {
        Task<Session?> GetByIdAsync(string id);
        Task<Session?> GetActiveSessionByUserIdAsync(string userId);
        Task<IEnumerable<Session>> GetSessionsByUserIdAsync(string userId);
        Task<IEnumerable<Session>> GetActiveSessionsAsync();
        Task<IEnumerable<Session>> GetSessionsByStatusAsync(SessionStatus status);
        Task<IEnumerable<Session>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Session> AddAsync(Session session);
        Task<Session> UpdateAsync(Session session);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<int> GetActiveSessionsCountAsync();
        Task<int> GetSharingSessionsCountAsync();
        Task<IEnumerable<Session>> GetSessionsWithRecordingsAsync();
        Task<TimeSpan> GetTotalSessionTimeByUserIdAsync(string userId);
    }
}