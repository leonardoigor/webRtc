using System;
using WebRtcServer.Domain.Enums;

namespace WebRtcServer.Domain.Entities
{
    /// <summary>
    /// Representa uma gravação de sessão de compartilhamento
    /// </summary>
    public class Recording
    {
        public string Id { get; private set; }
        public string SessionId { get; private set; }
        public string FilePath { get; private set; }
        public string FileName { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public bool IsRecording { get; private set; }
        public RecordingStatus Status { get; private set; }
        public long? FileSizeBytes { get; private set; }
        public TimeSpan? Duration { get; private set; }
        public string? ErrorMessage { get; private set; }
        public RecordingQuality Quality { get; private set; }

        private Recording() { } // Para EF Core

        public Recording(string sessionId, string filePath, RecordingQuality quality = RecordingQuality.Standard)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
            
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            Id = Guid.NewGuid().ToString();
            SessionId = sessionId;
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Quality = quality;
            StartTime = DateTime.UtcNow;
            IsRecording = true;
            Status = RecordingStatus.Recording;
        }

        public void Stop()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Recording is not active");

            IsRecording = false;
            EndTime = DateTime.UtcNow;
            Duration = EndTime - StartTime;
            Status = RecordingStatus.Completed;
            
            // Calcular tamanho do arquivo se existir
            if (File.Exists(FilePath))
            {
                var fileInfo = new FileInfo(FilePath);
                FileSizeBytes = fileInfo.Length;
            }
        }

        public void SetError(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));

            IsRecording = false;
            EndTime = DateTime.UtcNow;
            Duration = EndTime - StartTime;
            Status = RecordingStatus.Error;
            ErrorMessage = errorMessage;
        }

        public void Pause()
        {
            if (!IsRecording)
                throw new InvalidOperationException("Cannot pause inactive recording");

            Status = RecordingStatus.Paused;
        }

        public void Resume()
        {
            if (Status != RecordingStatus.Paused)
                throw new InvalidOperationException("Can only resume paused recordings");

            Status = RecordingStatus.Recording;
        }

        public void UpdateFileSize(long fileSizeBytes)
        {
            FileSizeBytes = fileSizeBytes;
        }

        public bool FileExists()
        {
            return File.Exists(FilePath);
        }

        public long GetCurrentFileSize()
        {
            if (!FileExists())
                return 0;

            var fileInfo = new FileInfo(FilePath);
            return fileInfo.Length;
        }

        public TimeSpan GetCurrentDuration()
        {
            if (Duration.HasValue)
                return Duration.Value;

            var endTime = EndTime ?? DateTime.UtcNow;
            return endTime - StartTime;
        }

        public string GetFormattedDuration()
        {
            var duration = GetCurrentDuration();
            return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        public string GetFormattedFileSize()
        {
            var sizeBytes = FileSizeBytes ?? GetCurrentFileSize();
            
            if (sizeBytes < 1024)
                return $"{sizeBytes} B";
            
            if (sizeBytes < 1024 * 1024)
                return $"{sizeBytes / 1024.0:F1} KB";
            
            if (sizeBytes < 1024 * 1024 * 1024)
                return $"{sizeBytes / (1024.0 * 1024.0):F1} MB";
            
            return $"{sizeBytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }
    }

    public enum RecordingStatus
    {
        Recording,
        Paused,
        Completed,
        Error,
        Cancelled
    }

}