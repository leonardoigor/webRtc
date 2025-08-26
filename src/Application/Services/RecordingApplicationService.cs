using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Application.Interfaces;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Interfaces;

namespace WebRtcServer.Application.Services
{
    /// <summary>
    /// Serviço de aplicação para gerenciamento de gravações
    /// </summary>
    public class RecordingApplicationService : IRecordingApplicationService
    {
        private readonly IRecordingRepository _recordingRepository;
        private readonly IRecordingService _recordingService;
        private readonly ISessionRepository _sessionRepository;

        public RecordingApplicationService(
            IRecordingRepository recordingRepository,
            IRecordingService recordingService,
            ISessionRepository sessionRepository)
        {
            _recordingRepository = recordingRepository ?? throw new ArgumentNullException(nameof(recordingRepository));
            _recordingService = recordingService ?? throw new ArgumentNullException(nameof(recordingService));
            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        }

        public async Task<RecordingDto> StartRecordingAsync(StartRecordingDto startRecordingDto)
        {
            // Verificar se a sessão existe
            var session = await _sessionRepository.GetByIdAsync(startRecordingDto.SessionId);
            if (session == null)
                throw new ArgumentException($"Session {startRecordingDto.SessionId} not found");

            // Verificar se já existe uma gravação ativa para a sessão
            var activeRecordings = await _recordingRepository.GetBySessionIdAsync(startRecordingDto.SessionId);
            if (activeRecordings.Any(r => r.IsRecording))
                throw new InvalidOperationException($"Session {startRecordingDto.SessionId} already has an active recording");

            var recording = await _recordingService.StartRecordingAsync(startRecordingDto.SessionId, startRecordingDto.Quality);
            
            // Adicionar gravação à sessão
            session.StartRecording(recording.FilePath);
            await _sessionRepository.UpdateAsync(session);

            return MapToDto(recording);
        }

        public async Task<bool> StopRecordingAsync(string recordingId)
        {
            var recording = await _recordingRepository.GetByIdAsync(recordingId);
            if (recording == null)
                return false;

            var success = await _recordingService.StopRecordingAsync(recordingId);
            if (success)
            {
                recording.Stop();
                await _recordingRepository.UpdateAsync(recording);
            }

            return success;
        }

        public async Task<bool> PauseRecordingAsync(string recordingId)
        {
            var recording = await _recordingRepository.GetByIdAsync(recordingId);
            if (recording == null)
                return false;

            var success = await _recordingService.PauseRecordingAsync(recordingId);
            if (success)
            {
                recording.Pause();
                await _recordingRepository.UpdateAsync(recording);
            }

            return success;
        }

        public async Task<bool> ResumeRecordingAsync(string recordingId)
        {
            var recording = await _recordingRepository.GetByIdAsync(recordingId);
            if (recording == null)
                return false;

            var success = await _recordingService.ResumeRecordingAsync(recordingId);
            if (success)
            {
                recording.Resume();
                await _recordingRepository.UpdateAsync(recording);
            }

            return success;
        }

        public async Task<RecordingDto?> GetRecordingByIdAsync(string recordingId)
        {
            var recording = await _recordingRepository.GetByIdAsync(recordingId);
            return recording != null ? MapToDto(recording) : null;
        }

        public async Task<IEnumerable<RecordingDto>> GetRecordingsBySessionIdAsync(string sessionId)
        {
            var recordings = await _recordingRepository.GetBySessionIdAsync(sessionId);
            return recordings.Select(MapToDto);
        }

        public async Task<IEnumerable<RecordingDto>> GetActiveRecordingsAsync()
        {
            var recordings = await _recordingRepository.GetActiveRecordingsAsync();
            return recordings.Select(MapToDto);
        }

        public async Task<IEnumerable<RecordingDto>> GetRecordingsByStatusAsync(RecordingStatus status)
        {
            var recordings = await _recordingRepository.GetRecordingsByStatusAsync(status);
            return recordings.Select(MapToDto);
        }

        public async Task<IEnumerable<RecordingDto>> GetCompletedRecordingsAsync()
        {
            var recordings = await _recordingRepository.GetCompletedRecordingsAsync();
            return recordings.Select(MapToDto);
        }

        public async Task<bool> DeleteRecordingAsync(string recordingId)
        {
            var recording = await _recordingRepository.GetByIdAsync(recordingId);
            if (recording == null)
                return false;

            var success = await _recordingService.DeleteRecordingAsync(recordingId);
            if (success)
            {
                await _recordingRepository.DeleteAsync(recordingId);
            }

            return success;
        }

        public async Task<bool> ValidateRecordingFileAsync(string recordingId)
        {
            var recording = await _recordingRepository.GetByIdAsync(recordingId);
            if (recording == null)
                return false;

            return await _recordingService.ValidateRecordingFileAsync(recording.FilePath);
        }

        public async Task<RecordingDto> UpdateRecordingAsync(string recordingId, UpdateRecordingDto updateRecordingDto)
        {
            var recording = await _recordingRepository.GetByIdAsync(recordingId);
            if (recording == null)
                throw new ArgumentException($"Recording with id {recordingId} not found");

            if (updateRecordingDto.Status.HasValue)
            {
                switch (updateRecordingDto.Status.Value)
                {
                    case RecordingStatus.Error:
                        recording.SetError(updateRecordingDto.ErrorMessage ?? "Unknown error");
                        break;
                    case RecordingStatus.Completed:
                        recording.Stop();
                        break;
                    case RecordingStatus.Paused:
                        recording.Pause();
                        break;
                    case RecordingStatus.Recording:
                        recording.Resume();
                        break;
                }
            }

            if (updateRecordingDto.FileSizeBytes.HasValue)
            {
                recording.UpdateFileSize(updateRecordingDto.FileSizeBytes.Value);
            }

            var updatedRecording = await _recordingRepository.UpdateAsync(recording);
            return MapToDto(updatedRecording);
        }

        public async Task<int> GetActiveRecordingsCountAsync()
        {
            return await _recordingRepository.GetActiveRecordingsCountAsync();
        }

        public async Task<long> GetTotalRecordingsSizeAsync()
        {
            return await _recordingRepository.GetTotalRecordingsSizeAsync();
        }

        public async Task<IEnumerable<RecordingDto>> GetRecordingsWithErrorsAsync()
        {
            var recordings = await _recordingRepository.GetRecordingsWithErrorsAsync();
            return recordings.Select(MapToDto);
        }

        public async Task<IEnumerable<RecordingDto>> GetAllRecordingsAsync()
        {
            var recordings = await _recordingRepository.GetAllAsync();
            return recordings.Select(MapToDto);
        }

        private static RecordingDto MapToDto(Recording recording)
        {
            return new RecordingDto
            {
                Id = recording.Id,
                SessionId = recording.SessionId,
                FilePath = recording.FilePath,
                FileName = recording.FileName,
                StartTime = recording.StartTime,
                EndTime = recording.EndTime,
                IsRecording = recording.IsRecording,
                Status = recording.Status,
                FileSizeBytes = recording.FileSizeBytes ?? 0,
                FormattedFileSize = recording.GetFormattedFileSize(),
                Duration = recording.Duration ?? TimeSpan.Zero,
                FormattedDuration = recording.GetFormattedDuration(),
                ErrorMessage = recording.ErrorMessage,
                Quality = recording.Quality
            };
        }
    }
}