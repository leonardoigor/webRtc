using System;

namespace WebRtcServer.Domain.Entities
{
    /// <summary>
    /// Representa uma conexão WebRTC entre usuários
    /// </summary>
    public class Connection
    {
        public string Id { get; private set; }
        public string SessionId { get; private set; }
        public string ConnectionId { get; private set; }
        public string? TargetUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public bool IsActive { get; private set; }
        public ConnectionStatus Status { get; private set; }
        public ConnectionType Type { get; private set; }
        public string? LastError { get; private set; }

        private Connection() { } // Para EF Core

        public Connection(string sessionId, string connectionId, string? targetUserId = null, ConnectionType type = ConnectionType.WebRTC)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
            
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Connection ID cannot be null or empty", nameof(connectionId));

            Id = Guid.NewGuid().ToString();
            SessionId = sessionId;
            ConnectionId = connectionId;
            TargetUserId = targetUserId;
            Type = type;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            Status = ConnectionStatus.Connecting;
        }

        public void SetConnected()
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot set connected status on inactive connection");

            Status = ConnectionStatus.Connected;
        }

        public void SetError(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Error message cannot be null or empty", nameof(error));

            LastError = error;
            Status = ConnectionStatus.Error;
        }

        public void Close()
        {
            IsActive = false;
            ClosedAt = DateTime.UtcNow;
            Status = ConnectionStatus.Closed;
        }

        public void SetDisconnected()
        {
            Status = ConnectionStatus.Disconnected;
        }

        public TimeSpan GetDuration()
        {
            var endTime = ClosedAt ?? DateTime.UtcNow;
            return endTime - CreatedAt;
        }

        public bool IsConnectedToUser(string userId)
        {
            return TargetUserId == userId && IsActive && Status == ConnectionStatus.Connected;
        }

        public void UpdateTargetUser(string targetUserId)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                throw new ArgumentException("Target user ID cannot be null or empty", nameof(targetUserId));

            TargetUserId = targetUserId;
        }
    }

    public enum ConnectionStatus
    {
        Connecting,
        Connected,
        Disconnected,
        Closed,
        Error
    }

    public enum ConnectionType
    {
        WebRTC,
        SignalR,
        Http
    }
}