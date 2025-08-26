using System;
using WebRtcServer.Domain.Entities;

namespace WebRtcServer.Application.DTOs
{
    /// <summary>
    /// DTO para transferência de dados de conexão
    /// </summary>
    public class ConnectionDto
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public string? TargetUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool IsActive { get; set; }
        public ConnectionStatus Status { get; set; }
        public ConnectionType Type { get; set; }
        public TimeSpan Duration { get; set; }
        public string? LastError { get; set; }
    }

    /// <summary>
    /// DTO para criação de conexão
    /// </summary>
    public class CreateConnectionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public string? TargetUserId { get; set; }
        public ConnectionType Type { get; set; }
    }

    /// <summary>
    /// DTO para atualização de conexão
    /// </summary>
    public class UpdateConnectionDto
    {
        public ConnectionStatus? Status { get; set; }
        public string? TargetUserId { get; set; }
        public string? LastError { get; set; }
    }
}