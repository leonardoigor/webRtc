using Microsoft.AspNetCore.SignalR;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Application.Interfaces;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;
using WebRtcServer.Domain.Interfaces;
using WebRtcServer.Domain.ValueObjects;
namespace WebRtcServer.Presentation.Hubs;

/// <summary>
/// Hub SignalR para WebRTC - Refatorado seguindo SRP
/// </summary>
public class WebRtcHub : Hub
{
    private readonly IUserService _userService;
    private readonly IStreamingService _streamingService;
    private readonly IWebRtcService _webRtcService;
    
    public WebRtcHub(
        IUserService userService,
        IStreamingService streamingService,
        IWebRtcService webRtcService)
    {
        _userService = userService;
        _streamingService = streamingService;
        _webRtcService = webRtcService;
    }
    
    public async Task RegisterUser(string userId)
    {
        Console.WriteLine($"[DEBUG] RegisterUser chamado - UserId: {userId}, ConnectionId: {Context.ConnectionId}, Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        
        var userDto = await _userService.RegisterUserAsync(new CreateUserDto
        {
            UserId = userId,
            ConnectionId = Context.ConnectionId,
            Type = UserType.WebClient
        });
        
        await _userService.SetUserOnlineAsync(userDto.UserId, Context.ConnectionId);
        Console.WriteLine($"[DEBUG] Usuário {userId} registrado e definido como online");
        
        // Notificar sobre atualização da lista
        var onlineUsers = await _userService.GetOnlineUsersAsync();
        await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
        Console.WriteLine($"[DEBUG] Lista de usuários online atualizada - Total: {onlineUsers.Count()}");
    }
    
    public async Task RegisterDesktopClient(string userId, string clientId, string groupId)
    {
        Console.WriteLine($"[DEBUG] RegisterDesktopClient chamado - UserId: {userId}, ConnectionId: {Context.ConnectionId}");
        
        // Primeiro, tentar encontrar um usuário existente com o mesmo userId truncado
        var existingUser = await _userService.GetUserByUserIdAsync(userId);
        Console.WriteLine($"[DEBUG] Usuário existente encontrado: {existingUser?.UserId ?? "null"}");
        
        if (existingUser != null)
        {
            Console.WriteLine($"[DEBUG] Atualizando ConnectionId do usuário existente: {existingUser.UserId}");
            // Atualizar o ConnectionId do usuário existente
            await _userService.SetUserOnlineAsync(existingUser.UserId, Context.ConnectionId);
        }
        else
        {
            Console.WriteLine($"[DEBUG] Criando novo usuário: {userId}");
            // Criar novo usuário se não existir
            var userDto = await _userService.RegisterUserAsync(new CreateUserDto
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                Type = UserType.DesktopClient,
                GroupId = groupId
            });
            
            await _userService.SetUserOnlineAsync(userDto.UserId, Context.ConnectionId);
        }
        
        Console.WriteLine($"[DEBUG] Usuário {userId} definido como online");
        
        // Notificar sobre atualização da lista
        var onlineUsers = await _userService.GetOnlineUsersAsync();
        await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
        Console.WriteLine($"[DEBUG] Lista de usuários online atualizada após registro");
    }
    
    public async Task<List<UserDto>> GetOnlineUsers()
    {
        var users = await _userService.GetOnlineUsersAsync();
        return users.ToList();
    }
    
    // Métodos de heartbeat para manter conexão ativa
    public async Task Ping()
    {
        Console.WriteLine($"[DEBUG] Ping recebido de ConnectionId: {Context.ConnectionId}, Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        
        // Atualizar atividade do usuário
        var user = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user != null)
        {
            await _userService.UpdateUserActivityAsync(user.UserId);
        }
        
        // Responder com pong
        await Clients.Caller.SendAsync("Pong", DateTime.Now);
    }
    
    public async Task KeepAlive()
    {
        Console.WriteLine($"[DEBUG] KeepAlive recebido de ConnectionId: {Context.ConnectionId}");
        
        // Atualizar atividade do usuário
        var user = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user != null)
        {
            await _userService.UpdateUserActivityAsync(user.UserId);
            Console.WriteLine($"[DEBUG] Atividade atualizada para usuário: {user.UserId}");
        }
        
        // Confirmar que está vivo
        await Clients.Caller.SendAsync("KeepAliveResponse", "OK");
    }
    
    public async Task SendOffer(string targetUserId, object offer)
    {
        var targetUser = await _userService.GetUserByUserIdAsync(targetUserId);
        if (targetUser != null)
        {
            await Clients.Client(targetUser.ConnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }
    }
    
    public async Task SendOfferToConnection(string targetConnectionId, object offer)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
    }
    
    public async Task SendAnswer(string targetConnectionId, object answer)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
    }
    
    public async Task SendIceCandidate(string targetConnectionId, object candidate)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
    }
    
    // Método SendVideoFrame removido - usando WebRTC real
    
    public async Task StartSharing(object offer)
    {
        Console.WriteLine($"[DEBUG] StartSharing chamado para ConnectionId: {Context.ConnectionId}");
        var user = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
        Console.WriteLine($"[DEBUG] Usuário encontrado: {user?.UserId ?? "null"}");
        
        if (user != null)
        {
            Console.WriteLine($"[DEBUG] Iniciando sessão para usuário: {user.UserId}");
            var sessionDto = await _streamingService.StartSessionAsync(user.UserId);
            Console.WriteLine($"[DEBUG] Sessão criada: {sessionDto.Id}");
            
            Console.WriteLine($"[DEBUG] Iniciando compartilhamento para sessão: {sessionDto.Id}");
            await _streamingService.StartSharingAsync(sessionDto.Id);
            Console.WriteLine($"[DEBUG] Compartilhamento iniciado com sucesso");
            
            // Processar oferta WebRTC
            var offerString = offer?.ToString() ?? "";
            await _webRtcService.CreateOfferAsync(Context.ConnectionId, offerString);
            
            // Notificar sobre atualização da lista
            var onlineUsers = await _userService.GetOnlineUsersAsync();
            await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
            Console.WriteLine($"[DEBUG] Lista de usuários online atualizada");
        }
        else
        {
            Console.WriteLine($"[DEBUG] ERRO: Usuário não encontrado para ConnectionId: {Context.ConnectionId}");
        }
    }
    
    public async Task RequestStream(string targetUserId)
    {
        var targetUser = await _userService.GetUserByUserIdAsync(targetUserId);
        if (targetUser != null)
        {
            // Solicitar que o usuário alvo envie uma oferta para este viewer
            await Clients.Client(targetUser.ConnectionId).SendAsync("StreamRequested", Context.ConnectionId);
        }
    }
    
    public async Task StopStream()
    {
        var user = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
        if (user != null)
        {
            var activeSessions = await _streamingService.GetSessionsByUserIdAsync(user.UserId);
            foreach (var session in activeSessions.Where(s => s.IsActive))
            {
                await _streamingService.StopSharingAsync(session.Id);
                await _streamingService.EndSessionAsync(session.Id);
                
                // Parar streaming WebRTC
                await _webRtcService.StopStreamingAsync(user.UserId, session.Id);
            }
            
            // Notificar sobre atualização da lista
            var onlineUsers = await _userService.GetOnlineUsersAsync();
            await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
        }
    }
    
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[DEBUG] OnConnectedAsync chamado - ConnectionId: {Context.ConnectionId}, Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine($"[DEBUG] User Agent: {Context.GetHttpContext()?.Request.Headers["User-Agent"].FirstOrDefault() ?? "N/A"}");
        Console.WriteLine($"[DEBUG] Remote IP: {Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "N/A"}");
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[DEBUG] OnDisconnectedAsync chamado para ConnectionId: {Context.ConnectionId}, Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        Console.WriteLine($"[DEBUG] Exception: {exception?.Message ?? "null"}");
        Console.WriteLine($"[DEBUG] Exception Type: {exception?.GetType().Name ?? "null"}");
        Console.WriteLine($"[DEBUG] Stack Trace: {exception?.StackTrace ?? "null"}");
        
        var user = await _userService.GetUserByConnectionIdAsync(Context.ConnectionId);
        Console.WriteLine($"[DEBUG] Usuário encontrado para desconexão: {user?.UserId ?? "null"}");
        
        if (user != null)
        {
            Console.WriteLine($"[DEBUG] Definindo usuário {user.UserId} como offline");
            await _userService.SetUserOfflineAsync(user.UserId);
            
            // Finalizar sessões ativas
            var activeSessions = await _streamingService.GetSessionsByUserIdAsync(user.UserId);
            Console.WriteLine($"[DEBUG] Finalizando {activeSessions.Count()} sessões ativas");
            foreach (var session in activeSessions)
            {
                Console.WriteLine($"[DEBUG] Finalizando sessão: {session.Id}");
                await _streamingService.EndSessionAsync(session.Id);
            }
            
            // Fechar conexões WebRTC
            await _webRtcService.CloseConnectionAsync(new ConnectionId(Context.ConnectionId));
            
            // Notificar sobre atualização da lista
            var onlineUsers = await _userService.GetOnlineUsersAsync();
            await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
            Console.WriteLine($"[DEBUG] Lista de usuários online atualizada após desconexão - Total restante: {onlineUsers.Count()}");
        }
        else
        {
            Console.WriteLine($"[DEBUG] AVISO: Nenhum usuário encontrado para ConnectionId na desconexão: {Context.ConnectionId}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}