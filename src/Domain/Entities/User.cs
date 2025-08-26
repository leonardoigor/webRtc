using System;
using System.Collections.Generic;
using WebRtcServer.Domain.Enums;

namespace WebRtcServer.Domain.Entities
{
    /// <summary>
    /// Representa um usuário no sistema WebRTC
    /// </summary>
    public class User
    {
        public string Id { get; private set; }
        public string UserId { get; private set; }
        public string ConnectionId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastActivity { get; private set; }
        public bool IsOnline { get; private set; }
        public UserType Type { get; private set; }
        public string? GroupId { get; private set; }
        
        private readonly List<Session> _sessions = new();
        public IReadOnlyList<Session> Sessions => _sessions.AsReadOnly();

        private User() { } // Para EF Core

        public User(string userId, string connectionId, UserType type = UserType.WebClient, string? groupId = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("Connection ID cannot be null or empty", nameof(connectionId));

            Id = Guid.NewGuid().ToString();
            UserId = userId;
            ConnectionId = connectionId;
            Type = type;
            GroupId = groupId;
            CreatedAt = DateTime.UtcNow;
            LastActivity = DateTime.UtcNow;
            IsOnline = true;
        }

        public void UpdateActivity()
        {
            LastActivity = DateTime.UtcNow;
        }

        public void SetOnline()
        {
            IsOnline = true;
            UpdateActivity();
        }

        public void SetOffline()
        {
            IsOnline = false;
            UpdateActivity();
        }

        public void UpdateConnectionId(string newConnectionId)
        {
            if (string.IsNullOrWhiteSpace(newConnectionId))
                throw new ArgumentException("Connection ID cannot be null or empty", nameof(newConnectionId));
            
            ConnectionId = newConnectionId;
            UpdateActivity();
        }

        public void UpdateGroupId(string? groupId)
        {
            GroupId = groupId;
            UpdateActivity();
        }

        public Session StartSession()
        {
            var session = new Session(Id);
            _sessions.Add(session);
            return session;
        }

        public void AddSession(Session session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            
            _sessions.Add(session);
            UpdateActivity();
        }

        public void EndCurrentSession()
        {
            var currentSession = _sessions.FirstOrDefault(s => s.IsActive);
            currentSession?.End();
        }

        public Session? GetCurrentSession()
        {
            return _sessions.FirstOrDefault(s => s.IsActive);
        }
    }
}