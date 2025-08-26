using System.Collections.Generic;
using System.Threading.Tasks;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Domain.Entities;

namespace WebRtcServer.Application.Interfaces
{
    /// <summary>
    /// Interface para serviços de gravação na camada de aplicação
    /// </summary>
    public interface IRecordingApplicationService
    {
        Task<RecordingDto> StartRecordingAsync(StartRecordingDto startRecordingDto);
        Task<bool> StopRecordingAsync(string recordingId);
        Task<bool> PauseRecordingAsync(string recordingId);
        Task<bool> ResumeRecordingAsync(string recordingId);
        Task<RecordingDto?> GetRecordingByIdAsync(string recordingId);
        Task<IEnumerable<RecordingDto>> GetRecordingsBySessionIdAsync(string sessionId);
        Task<IEnumerable<RecordingDto>> GetActiveRecordingsAsync();
        Task<IEnumerable<RecordingDto>> GetRecordingsByStatusAsync(RecordingStatus status);
        Task<IEnumerable<RecordingDto>> GetCompletedRecordingsAsync();
        Task<bool> DeleteRecordingAsync(string recordingId);
        Task<bool> ValidateRecordingFileAsync(string recordingId);
        Task<RecordingDto> UpdateRecordingAsync(string recordingId, UpdateRecordingDto updateRecordingDto);
        Task<int> GetActiveRecordingsCountAsync();
        Task<long> GetTotalRecordingsSizeAsync();
        Task<IEnumerable<RecordingDto>> GetRecordingsWithErrorsAsync();
        Task<IEnumerable<RecordingDto>> GetAllRecordingsAsync();
    }
}