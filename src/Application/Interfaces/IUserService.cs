using System.Collections.Generic;
using System.Threading.Tasks;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;

namespace WebRtcServer.Application.Interfaces
{
    /// <summary>
    /// Interface para serviços de usuário na camada de aplicação
    /// </summary>
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<UserDto?> GetUserByUserIdAsync(string userId);
        Task<UserDto?> GetUserByConnectionIdAsync(string connectionId);
        Task<IEnumerable<UserDto>> GetOnlineUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByTypeAsync(UserType userType);
        Task<IEnumerable<UserDto>> GetUsersByGroupIdAsync(string groupId);
        Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> SetUserOnlineAsync(string userId, string connectionId);
        Task<bool> SetUserOfflineAsync(string userId);
        Task<bool> UpdateUserActivityAsync(string userId);
        Task<int> GetOnlineUsersCountAsync();
        Task<IEnumerable<UserDto>> GetUsersWithActiveSessionsAsync();
        Task<bool> UserExistsAsync(string userId);
    }
}