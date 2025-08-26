using System;
using System.Collections.Generic;
using WebRtcServer.Domain.Entities;

namespace WebRtcServer.Application.DTOs
{
    /// <summary>
    /// DTO para transferência de dados de sessão
    /// </summary>
    public class SessionDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsSharing { get; set; }
        public SessionStatus Status { get; set; }
        public TimeSpan Duration { get; set; }
        public int ActiveConnectionsCount { get; set; }
        public List<ConnectionDto> Connections { get; set; } = new();
        public List<RecordingDto> Recordings { get; set; } = new();
    }

    /// <summary>
    /// DTO para criação de sessão
    /// </summary>
    public class CreateSessionDto
    {
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para atualização de sessão
    /// </summary>
    public class UpdateSessionDto
    {
        public bool? IsSharing { get; set; }
        public SessionStatus? Status { get; set; }
    }
}