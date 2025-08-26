using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Application.Interfaces;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;
using WebRtcServer.Domain.Interfaces;
using WebRtcServer.Domain.ValueObjects;

namespace WebRtcServer.Application.Services
{
    /// <summary>
    /// Serviço de aplicação para gerenciamento de usuários
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetUserByUserIdAsync(string userId)
        {
            var user = await _userRepository.GetByUserIdAsync(new UserId(userId));
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetUserByConnectionIdAsync(string connectionId)
        {
            var user = await _userRepository.GetByConnectionIdAsync(new ConnectionId(connectionId));
            return user != null ? MapToDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetOnlineUsersAsync()
        {
            var users = await _userRepository.GetOnlineUsersAsync();
            return users.Select(MapToDto);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByTypeAsync(UserType userType)
        {
            var users = await _userRepository.GetUsersByTypeAsync(userType);
            return users.Select(MapToDto);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByGroupIdAsync(string groupId)
        {
            var users = await _userRepository.GetUsersByGroupIdAsync(groupId);
            return users.Select(MapToDto);
        }

        public async Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto)
        {
            var user = new User(
                new UserId(createUserDto.UserId),
                new ConnectionId(createUserDto.ConnectionId),
                createUserDto.Type,
                createUserDto.GroupId
            );

            var createdUser = await _userRepository.AddAsync(user);
            return MapToDto(createdUser);
        }

        public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ArgumentException($"User with id {id} not found");

            if (updateUserDto.ConnectionId != null)
                user.UpdateConnectionId(new ConnectionId(updateUserDto.ConnectionId));

            if (updateUserDto.IsOnline.HasValue)
            {
                if (updateUserDto.IsOnline.Value)
                    user.SetOnline();
                else
                    user.SetOffline();
            }

            if (updateUserDto.GroupId != null)
                user.UpdateGroupId(updateUserDto.GroupId);

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var exists = await _userRepository.ExistsAsync(id);
            if (!exists)
                return false;

            await _userRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> SetUserOnlineAsync(string userId, string connectionId)
        {
            var user = await _userRepository.GetByUserIdAsync(new UserId(userId));
            if (user == null)
                return false;

            user.UpdateConnectionId(new ConnectionId(connectionId));
            user.SetOnline();
            user.UpdateActivity();

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> SetUserOfflineAsync(string userId)
        {
            var user = await _userRepository.GetByUserIdAsync(new UserId(userId));
            if (user == null)
                return false;

            user.SetOffline();
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateUserActivityAsync(string userId)
        {
            var user = await _userRepository.GetByUserIdAsync(new UserId(userId));
            if (user == null)
                return false;

            user.UpdateActivity();
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<int> GetOnlineUsersCountAsync()
        {
            return await _userRepository.GetOnlineUsersCountAsync();
        }

        public async Task<IEnumerable<UserDto>> GetUsersWithActiveSessionsAsync()
        {
            var users = await _userRepository.GetUsersWithActiveSessionsAsync();
            return users.Select(MapToDto);
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            return await _userRepository.ExistsByUserIdAsync(new UserId(userId));
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserId = user.UserId,
                ConnectionId = user.ConnectionId,
                CreatedAt = user.CreatedAt,
                LastActivity = user.LastActivity ?? DateTime.MinValue,
                IsOnline = user.IsOnline,
                Type = user.Type,
                GroupId = user.GroupId,
                Sessions = user.Sessions.Select(MapSessionToDto).ToList()
            };
        }

        private static SessionDto MapSessionToDto(Session session)
        {
            return new SessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                IsActive = session.IsActive,
                IsSharing = session.IsSharing,
                Status = session.Status,
                Duration = session.GetDuration(),
                ActiveConnectionsCount = session.GetActiveConnectionsCount()
            };
        }
    }
}