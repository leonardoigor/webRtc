using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Application.Interfaces;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Interfaces;
using WebRtcServer.Domain.ValueObjects;

namespace WebRtcServer.Application.Services
{
    /// <summary>
    /// Serviço de aplicação para gerenciamento de streaming
    /// </summary>
    public class StreamingService : IStreamingService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IConnectionRepository _connectionRepository;
        private readonly IWebRtcService _webRtcService;
        private readonly IUserRepository _userRepository;

        public StreamingService(
            ISessionRepository sessionRepository,
            IConnectionRepository connectionRepository,
            IWebRtcService webRtcService,
            IUserRepository userRepository)
        {
            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            _connectionRepository = connectionRepository ?? throw new ArgumentNullException(nameof(connectionRepository));
            _webRtcService = webRtcService ?? throw new ArgumentNullException(nameof(webRtcService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<SessionDto> StartSessionAsync(string userId)
        {
            // Verificar se já existe uma sessão ativa para o usuário e encerrá-la
            var existingSession = await _sessionRepository.GetActiveSessionByUserIdAsync(userId);
            if (existingSession != null)
            {
                existingSession.End();
                await _sessionRepository.UpdateAsync(existingSession);
            }

            var session = new Session(userId);
            var createdSession = await _sessionRepository.AddAsync(session);
            
            // Adicionar sessão ao usuário
            var user = await _userRepository.GetByUserIdAsync(new UserId(userId));
            if (user != null)
            {
                user.AddSession(createdSession);
                await _userRepository.UpdateAsync(user);
            }

            return MapSessionToDto(createdSession);
        }

        public async Task<bool> EndSessionAsync(string sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                return false;

            session.End();
            await _sessionRepository.UpdateAsync(session);
            return true;
        }

        public async Task<bool> StartSharingAsync(string sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                return false;

            session.StartSharing();
            await _sessionRepository.UpdateAsync(session);
            
            await _webRtcService.StartStreamingAsync(session.UserId, sessionId);
            return true;
        }

        public async Task<bool> StopSharingAsync(string sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                return false;

            session.StopSharing();
            await _sessionRepository.UpdateAsync(session);
            
            await _webRtcService.StopStreamingAsync(session.UserId, sessionId);
            return true;
        }

        public async Task<SessionDto?> GetSessionByIdAsync(string sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            return session != null ? MapSessionToDto(session) : null;
        }

        public async Task<SessionDto?> GetActiveSessionByUserIdAsync(string userId)
        {
            var session = await _sessionRepository.GetActiveSessionByUserIdAsync(userId);
            return session != null ? MapSessionToDto(session) : null;
        }

        public async Task<IEnumerable<SessionDto>> GetSessionsByUserIdAsync(string userId)
        {
            var sessions = await _sessionRepository.GetSessionsByUserIdAsync(userId);
            return sessions.Select(MapSessionToDto);
        }

        public async Task<IEnumerable<SessionDto>> GetActiveSessionsAsync()
        {
            var sessions = await _sessionRepository.GetActiveSessionsAsync();
            return sessions.Select(MapSessionToDto);
        }

        public async Task<ConnectionDto> CreateConnectionAsync(CreateConnectionDto createConnectionDto)
        {
            var connectionId = new ConnectionId(createConnectionDto.ConnectionId);
            var connection = new Connection(
                createConnectionDto.SessionId,
                connectionId,
                createConnectionDto.TargetUserId,
                createConnectionDto.Type
            );

            var createdConnection = await _connectionRepository.AddAsync(connection);
            
            // Adicionar conexão à sessão
            var session = await _sessionRepository.GetByIdAsync(createConnectionDto.SessionId);
            if (session != null)
            {
                session.AddConnection(createConnectionDto.ConnectionId, createConnectionDto.TargetUserId);
                await _sessionRepository.UpdateAsync(session);
            }

            return MapConnectionToDto(createdConnection);
        }

        public async Task<bool> CloseConnectionAsync(string connectionId)
        {
            var connection = await _connectionRepository.GetByConnectionIdAsync(new ConnectionId(connectionId));
            if (connection == null)
                return false;

            connection.Close();
            await _connectionRepository.UpdateAsync(connection);
            
            await _webRtcService.CloseConnectionAsync(new ConnectionId(connectionId));
            return true;
        }

        public async Task<IEnumerable<ConnectionDto>> GetConnectionsBySessionIdAsync(string sessionId)
        {
            var connections = await _connectionRepository.GetBySessionIdAsync(sessionId);
            return connections.Select(MapConnectionToDto);
        }

        public async Task<IEnumerable<ConnectionDto>> GetActiveConnectionsAsync()
        {
            var connections = await _connectionRepository.GetActiveConnectionsAsync();
            return connections.Select(MapConnectionToDto);
        }

        public async Task<bool> HandleWebRtcOfferAsync(string sessionId, string offer)
        {
            if (!await _webRtcService.ValidateOfferAsync(offer))
                return false;

            return await _webRtcService.CreateOfferAsync(sessionId, offer);
        }

        public async Task<bool> HandleWebRtcAnswerAsync(string sessionId, string answer)
        {
            if (!await _webRtcService.ValidateAnswerAsync(answer))
                return false;

            return await _webRtcService.CreateAnswerAsync(sessionId, answer);
        }

        public async Task<bool> HandleIceCandidateAsync(string sessionId, string candidate)
        {
            if (!await _webRtcService.ValidateIceCandidateAsync(candidate))
                return false;

            return await _webRtcService.AddIceCandidateAsync(sessionId, candidate);
        }

        public async Task<int> GetActiveSessionsCountAsync()
        {
            return await _sessionRepository.GetActiveSessionsCountAsync();
        }

        public async Task<int> GetSharingSessionsCountAsync()
        {
            return await _sessionRepository.GetSharingSessionsCountAsync();
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
                ActiveConnectionsCount = session.GetActiveConnectionsCount(),
                Connections = session.Connections.Select(MapConnectionToDto).ToList(),
                Recordings = session.Recordings.Select(MapRecordingToDto).ToList()
            };
        }

        private static ConnectionDto MapConnectionToDto(Connection connection)
        {
            return new ConnectionDto
            {
                Id = connection.Id,
                SessionId = connection.SessionId,
                ConnectionId = connection.ConnectionId,
                TargetUserId = connection.TargetUserId,
                CreatedAt = connection.CreatedAt,
                ClosedAt = connection.ClosedAt,
                IsActive = connection.IsActive,
                Status = connection.Status,
                Type = connection.Type,
                Duration = connection.GetDuration(),
                LastError = connection.LastError
            };
        }

        private static RecordingDto MapRecordingToDto(Recording recording)
        {
            return new RecordingDto
            {
                Id = recording.Id,
                SessionId = recording.SessionId,
                FilePath = recording.FilePath,
                FileName = recording.FileName,
                StartTime = recording.StartTime,
                EndTime = recording.EndTime,
                IsRecording = recording.IsRecording,
                Status = recording.Status,
                FileSizeBytes = recording.FileSizeBytes ?? 0,
                FormattedFileSize = recording.GetFormattedFileSize(),
                Duration = recording.Duration ?? TimeSpan.Zero,
                FormattedDuration = recording.GetFormattedDuration(),
                ErrorMessage = recording.ErrorMessage,
                Quality = recording.Quality
            };
        }
    }
}