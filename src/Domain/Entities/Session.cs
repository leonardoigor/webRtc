using System;
using System.Collections.Generic;

namespace WebRtcServer.Domain.Entities
{
    /// <summary>
    /// Representa uma sessão de compartilhamento de tela
    /// </summary>
    public class Session
    {
        public string Id { get; private set; }
        public string UserId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsSharing { get; private set; }
        public SessionStatus Status { get; private set; }
        
        private readonly List<Connection> _connections = new();
        public IReadOnlyList<Connection> Connections => _connections.AsReadOnly();
        
        private readonly List<Recording> _recordings = new();
        public IReadOnlyList<Recording> Recordings => _recordings.AsReadOnly();

        private Session() { } // Para EF Core

        public Session(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            Id = Guid.NewGuid().ToString();
            UserId = userId;
            StartTime = DateTime.UtcNow;
            IsActive = true;
            IsSharing = false;
            Status = SessionStatus.Created;
        }

        public void StartSharing()
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot start sharing on inactive session");

            IsSharing = true;
            Status = SessionStatus.Sharing;
        }

        public void StopSharing()
        {
            IsSharing = false;
            Status = SessionStatus.Connected;
        }

        public void End()
        {
            IsActive = false;
            IsSharing = false;
            EndTime = DateTime.UtcNow;
            Status = SessionStatus.Ended;
            
            // Finalizar todas as conexões ativas
            foreach (var connection in _connections.Where(c => c.IsActive))
            {
                connection.Close();
            }
            
            // Finalizar todas as gravações ativas
            foreach (var recording in _recordings.Where(r => r.IsRecording))
            {
                recording.Stop();
            }
        }

        public Connection AddConnection(string connectionId, string? targetUserId = null)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Connection ID cannot be null or empty", nameof(connectionId));

            var connection = new Connection(Id, connectionId, targetUserId);
            _connections.Add(connection);
            return connection;
        }

        public Recording StartRecording(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var recording = new Recording(Id, filePath);
            _recordings.Add(recording);
            return recording;
        }

        public TimeSpan GetDuration()
        {
            var endTime = EndTime ?? DateTime.UtcNow;
            return endTime - StartTime;
        }

        public int GetActiveConnectionsCount()
        {
            return _connections.Count(c => c.IsActive);
        }

        public Connection? GetConnection(string connectionId)
        {
            return _connections.FirstOrDefault(c => c.ConnectionId == connectionId);
        }
    }

    public enum SessionStatus
    {
        Created,
        Connected,
        Sharing,
        Ended,
        Error
    }
}