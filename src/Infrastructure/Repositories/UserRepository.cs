using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;
using WebRtcServer.Domain.Interfaces;
using WebRtcServer.Domain.ValueObjects;

namespace WebRtcServer.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação em memória do repositório de usuários
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<string, User> _users = new();

        public Task<User?> GetByIdAsync(string id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<User?> GetByUserIdAsync(UserId userId)
        {
            var user = _users.Values.FirstOrDefault(u => u.UserId == userId);
            return Task.FromResult(user);
        }

        public Task<User?> GetByConnectionIdAsync(ConnectionId connectionId)
        {
            var user = _users.Values.FirstOrDefault(u => u.ConnectionId == connectionId);
            return Task.FromResult(user);
        }

        public Task<IEnumerable<User>> GetOnlineUsersAsync()
        {
            var onlineUsers = _users.Values.Where(u => u.IsOnline).ToList();
            return Task.FromResult<IEnumerable<User>>(onlineUsers);
        }

        public Task<IEnumerable<User>> GetUsersByTypeAsync(UserType userType)
        {
            var users = _users.Values.Where(u => u.Type == userType).ToList();
            return Task.FromResult<IEnumerable<User>>(users);
        }

        public Task<IEnumerable<User>> GetUsersByGroupIdAsync(string groupId)
        {
            var users = _users.Values.Where(u => u.GroupId == groupId).ToList();
            return Task.FromResult<IEnumerable<User>>(users);
        }

        public Task<User> AddAsync(User user)
        {
            _users.TryAdd(user.Id, user);
            return Task.FromResult(user);
        }

        public Task<User> UpdateAsync(User user)
        {
            _users.TryUpdate(user.Id, user, _users[user.Id]);
            return Task.FromResult(user);
        }

        public Task DeleteAsync(string id)
        {
            _users.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string id)
        {
            return Task.FromResult(_users.ContainsKey(id));
        }

        public Task<bool> ExistsByUserIdAsync(UserId userId)
        {
            var exists = _users.Values.Any(u => u.UserId == userId);
            return Task.FromResult(exists);
        }

        public Task<bool> ExistsByConnectionIdAsync(ConnectionId connectionId)
        {
            var exists = _users.Values.Any(u => u.ConnectionId == connectionId);
            return Task.FromResult(exists);
        }

        public Task<int> GetOnlineUsersCountAsync()
        {
            var count = _users.Values.Count(u => u.IsOnline);
            return Task.FromResult(count);
        }

        public Task<IEnumerable<User>> GetUsersWithActiveSessionsAsync()
        {
            var users = _users.Values.Where(u => u.Sessions.Any(s => s.IsActive)).ToList();
            return Task.FromResult<IEnumerable<User>>(users);
        }
    }
}