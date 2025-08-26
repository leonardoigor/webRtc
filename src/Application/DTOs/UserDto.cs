using System;
using System.Collections.Generic;
using System.Linq;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;

namespace WebRtcServer.Application.DTOs
{
    /// <summary>
    /// DTO para transferência de dados de usuário
    /// </summary>
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? ConnectionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsOnline { get; set; }
        public UserType Type { get; set; }
        public string? GroupId { get; set; }
        public List<SessionDto> Sessions { get; set; } = new();
        
        /// <summary>
        /// Indica se o usuário está compartilhando tela atualmente
        /// </summary>
        public bool IsSharing => Sessions.Any(s => s.IsActive && s.IsSharing);
    }

    /// <summary>
    /// DTO para criação de usuário
    /// </summary>
    public class CreateUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public UserType Type { get; set; }
        public string? GroupId { get; set; }
    }

    /// <summary>
    /// DTO para atualização de usuário
    /// </summary>
    public class UpdateUserDto
    {
        public string? ConnectionId { get; set; }
        public bool? IsOnline { get; set; }
        public string? GroupId { get; set; }
    }
}