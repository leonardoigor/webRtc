using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;
using WebRtcServer.Domain.Interfaces;

namespace WebRtcServer.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação em memória do repositório de gravações
    /// </summary>
    public class RecordingRepository : IRecordingRepository
    {
        private readonly ConcurrentDictionary<string, Recording> _recordings = new();

        public Task<Recording?> GetByIdAsync(string id)
        {
            _recordings.TryGetValue(id, out var recording);
            return Task.FromResult(recording);
        }

        public Task<Recording?> GetByFilePathAsync(string filePath)
        {
            var recording = _recordings.Values.FirstOrDefault(r => r.FilePath == filePath);
            return Task.FromResult(recording);
        }

        public Task<IEnumerable<Recording>> GetAllAsync()
        {
            var recordings = _recordings.Values.ToList();
            return Task.FromResult<IEnumerable<Recording>>(recordings);
        }

        public Task<IEnumerable<Recording>> GetBySessionIdAsync(string sessionId)
        {
            var recordings = _recordings.Values.Where(r => r.SessionId == sessionId).ToList();
            return Task.FromResult<IEnumerable<Recording>>(recordings);
        }

        public Task<IEnumerable<Recording>> GetActiveRecordingsAsync()
        {
            var recordings = _recordings.Values.Where(r => r.IsRecording).ToList();
            return Task.FromResult<IEnumerable<Recording>>(recordings);
        }

        public Task<IEnumerable<Recording>> GetRecordingsByStatusAsync(RecordingStatus status)
        {
            var recordings = _recordings.Values.Where(r => r.Status == status).ToList();
            return Task.FromResult<IEnumerable<Recording>>(recordings);
        }

        public async Task<IEnumerable<Recording>> GetRecordingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            await Task.CompletedTask;
            return _recordings.Values
                .Where(r => r.StartTime >= startDate && r.StartTime <= endDate)
                .ToList();
        }

        public async Task<IEnumerable<Recording>> GetRecordingsByQualityAsync(RecordingQuality quality)
        {
            await Task.CompletedTask;
            return _recordings.Values
                .Where(r => r.Quality == quality)
                .ToList();
        }

        public Task<Recording> AddAsync(Recording recording)
        {
            _recordings.TryAdd(recording.Id, recording);
            return Task.FromResult(recording);
        }

        public Task<Recording> UpdateAsync(Recording recording)
        {
            _recordings.TryUpdate(recording.Id, recording, _recordings[recording.Id]);
            return Task.FromResult(recording);
        }

        public Task DeleteAsync(string id)
        {
            _recordings.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string id)
        {
            return Task.FromResult(_recordings.ContainsKey(id));
        }

        public Task<bool> ExistsByFilePathAsync(string filePath)
        {
            var exists = _recordings.Values.Any(r => r.FilePath == filePath);
            return Task.FromResult(exists);
        }

        public Task<int> GetActiveRecordingsCountAsync()
        {
            var count = _recordings.Values.Count(r => r.IsRecording);
            return Task.FromResult(count);
        }

        public Task<long> GetTotalRecordingsSizeAsync()
        {
            var totalSize = _recordings.Values.Sum(r => r.FileSizeBytes ?? 0);
            return Task.FromResult(totalSize);
        }

        public Task<IEnumerable<Recording>> GetCompletedRecordingsAsync()
        {
            var recordings = _recordings.Values.Where(r => r.Status == RecordingStatus.Completed).ToList();
            return Task.FromResult<IEnumerable<Recording>>(recordings);
        }

        public Task<IEnumerable<Recording>> GetRecordingsWithErrorsAsync()
        {
            var recordings = _recordings.Values.Where(r => r.Status == RecordingStatus.Error).ToList();
            return Task.FromResult<IEnumerable<Recording>>(recordings);
        }
    }
}