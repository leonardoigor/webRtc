using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;

namespace WebRtcServer.Domain.Interfaces
{
    /// <summary>
    /// Interface para repositório de gravações
    /// </summary>
    public interface IRecordingRepository
    {
        Task<Recording?> GetByIdAsync(string id);
        Task<Recording?> GetByFilePathAsync(string filePath);
        Task<IEnumerable<Recording>> GetAllAsync();
        Task<IEnumerable<Recording>> GetBySessionIdAsync(string sessionId);
        Task<IEnumerable<Recording>> GetActiveRecordingsAsync();
        Task<IEnumerable<Recording>> GetRecordingsByStatusAsync(RecordingStatus status);
        Task<IEnumerable<Recording>> GetRecordingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Recording>> GetRecordingsByQualityAsync(RecordingQuality quality);
        Task<Recording> AddAsync(Recording recording);
        Task<Recording> UpdateAsync(Recording recording);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> ExistsByFilePathAsync(string filePath);
        Task<int> GetActiveRecordingsCountAsync();
        Task<long> GetTotalRecordingsSizeAsync();
        Task<IEnumerable<Recording>> GetCompletedRecordingsAsync();
        Task<IEnumerable<Recording>> GetRecordingsWithErrorsAsync();
    }
}