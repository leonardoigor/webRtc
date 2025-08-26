using System.Collections.Generic;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;
using WebRtcServer.Domain.Interfaces;

namespace WebRtcServer.Domain.Interfaces{
    /// <summary>
    /// Interface para serviços de gravação
    /// </summary>
    public interface IRecordingService
    {
        Task<Recording> StartRecordingAsync(string sessionId, RecordingQuality quality = RecordingQuality.Medium);
        Task<bool> StopRecordingAsync(string recordingId);
        Task<bool> PauseRecordingAsync(string recordingId);
        Task<bool> ResumeRecordingAsync(string recordingId);
        Task<Recording?> GetRecordingAsync(string recordingId);
        Task<IEnumerable<Recording>> GetRecordingsBySessionAsync(string sessionId);
        Task<bool> DeleteRecordingAsync(string recordingId);
        Task<bool> ValidateRecordingFileAsync(string filePath);
        Task<long> GetRecordingFileSizeAsync(string filePath);
        Task<bool> ConvertRecordingAsync(string recordingId, string outputFormat);
        Task<string> GenerateRecordingThumbnailAsync(string recordingId);
        Task<bool> CompressRecordingAsync(string recordingId);
    }
}