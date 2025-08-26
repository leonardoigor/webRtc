using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;
using WebRtcServer.Domain.ValueObjects;

namespace WebRtcServer.Domain.Interfaces
{
    /// <summary>
    /// Interface para repositório de usuários
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByUserIdAsync(UserId userId);
        Task<User?> GetByConnectionIdAsync(ConnectionId connectionId);
        Task<IEnumerable<User>> GetOnlineUsersAsync();
        Task<IEnumerable<User>> GetUsersByTypeAsync(UserType userType);
        Task<IEnumerable<User>> GetUsersByGroupIdAsync(string groupId);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> ExistsByUserIdAsync(UserId userId);
        Task<bool> ExistsByConnectionIdAsync(ConnectionId connectionId);
        Task<int> GetOnlineUsersCountAsync();
        Task<IEnumerable<User>> GetUsersWithActiveSessionsAsync();
    }
}