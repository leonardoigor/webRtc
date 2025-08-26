using System;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;

namespace WebRtcServer.Application.DTOs
{
    /// <summary>
    /// DTO para transferência de dados de gravação
    /// </summary>
    public class RecordingDto
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsRecording { get; set; }
        public RecordingStatus Status { get; set; }
        public long FileSizeBytes { get; set; }
        public string FormattedFileSize { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string FormattedDuration { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public RecordingQuality Quality { get; set; }
    }

    /// <summary>
    /// DTO para iniciar gravação
    /// </summary>
    public class StartRecordingDto
    {
        public string SessionId { get; set; } = string.Empty;
        public RecordingQuality Quality { get; set; } = RecordingQuality.Medium;
    }

    /// <summary>
    /// DTO para atualização de gravação
    /// </summary>
    public class UpdateRecordingDto
    {
        public RecordingStatus? Status { get; set; }
        public string? ErrorMessage { get; set; }
        public long? FileSizeBytes { get; set; }
    }
}