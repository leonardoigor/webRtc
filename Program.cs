using Microsoft.AspNetCore.SignalR;
using SIPSorcery.Net;
using SIPSorcery.Media;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using FFMpegCore;
using FFMpegCore.Pipes;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddSignalR();
builder.Services.AddSingleton<WebRtcService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:8000", "http://127.0.0.1:8000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configurar pipeline
app.UseCors();
app.UseStaticFiles();
app.MapHub<WebRtcHub>("/webrtchub");

// Endpoint de status do servidor
app.MapGet("/", () => Results.Json(new { 
    status = "WebRTC Server Running", 
    timestamp = DateTime.Now,
    endpoints = new[] { "/webrtchub", "/api/recordings", "/recordings/{filename}" }
}));

// API para listar gravações
app.MapGet("/api/recordings", () =>
{
    var recordingsPath = Path.Combine(Directory.GetCurrentDirectory(), "recordings");
    if (!Directory.Exists(recordingsPath))
        return Results.Json(new string[0]);
    
    var files = Directory.GetFiles(recordingsPath, "*.mp4")
                        .Select(Path.GetFileName)
                        .ToArray();
    return Results.Json(files);
});

// Servir arquivos de gravação
app.MapGet("/recordings/{filename}", (string filename) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "recordings", filename);
    if (!File.Exists(filePath))
        return Results.NotFound();
    
    return Results.File(filePath, "video/mp4");
});

app.Run();

// Hub SignalR para comunicação WebRTC
public class WebRtcHub : Hub
{
    private readonly WebRtcService _webRtcService;
    
    public WebRtcHub(WebRtcService webRtcService)
    {
        _webRtcService = webRtcService;
    }
    
    public async Task RegisterUser(string userId)
    {
        await _webRtcService.RegisterUser(Context.ConnectionId, userId);
        
        // Notificar todos os clientes sobre a atualização da lista de usuários
        var onlineUsers = await _webRtcService.GetOnlineUsers();
        await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
    }
    
    public async Task<List<OnlineUser>> GetOnlineUsers()
    {
        return await _webRtcService.GetOnlineUsers();
    }
    
    public async Task SendOffer(object offer)
    {
        await _webRtcService.HandleOffer(Context.ConnectionId, offer);
        
        // Enviar oferta apenas para o viewer que solicitou (se houver um mapeamento)
        var targetViewer = _webRtcService.GetPendingViewer(Context.ConnectionId);
        if (!string.IsNullOrEmpty(targetViewer))
        {
            await Clients.Client(targetViewer).SendAsync("ReceiveOffer", offer);
            _webRtcService.ClearPendingViewer(Context.ConnectionId);
        }
        else
        {
            // Comportamento normal: broadcast para todos (quando não há viewer específico)
            await Clients.Others.SendAsync("ReceiveOffer", offer);
        }
        
        // Notificar sobre atualização da lista (usuário começou a compartilhar)
        var onlineUsers = await _webRtcService.GetOnlineUsers();
        await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
    }
    
    public async Task SendAnswer(object answer)
    {
        await Clients.Others.SendAsync("ReceiveAnswer", answer);
    }
    
    public async Task SendIceCandidate(object candidate)
    {
        await Clients.Others.SendAsync("ReceiveIceCandidate", candidate);
    }
    
    public async Task RequestStream(string targetUserId)
    {
        // Encontrar a conexão do usuário alvo
        var targetConnection = _webRtcService.GetUserConnection(targetUserId);
        if (targetConnection != null)
        {
            // Mapear o sharer para este viewer
            _webRtcService.SetPendingViewer(targetConnection, Context.ConnectionId);
            
            // Solicitar que o usuário alvo envie uma oferta para este viewer
            await Clients.Client(targetConnection).SendAsync("StreamRequested", Context.ConnectionId);
        }
    }
    
    public async Task StopStream()
    {
        await _webRtcService.StopStream(Context.ConnectionId);
        
        // Notificar sobre atualização da lista (usuário parou de compartilhar)
        var onlineUsers = await _webRtcService.GetOnlineUsers();
        await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _webRtcService.RemoveClient(Context.ConnectionId);
        
        // Notificar sobre atualização da lista (usuário desconectou)
        var onlineUsers = await _webRtcService.GetOnlineUsers();
        await Clients.All.SendAsync("OnlineUsersUpdated", onlineUsers);
        
        await base.OnDisconnectedAsync(exception);
    }
}

// Serviço WebRTC para gerenciar conexões e gravações
public class WebRtcService
{
    private readonly ConcurrentDictionary<string, ClientConnection> _clients = new();
    private readonly ConcurrentDictionary<string, UserSession> _userSessions = new();
    private readonly string _recordingsPath;
    
    public WebRtcService()
    {
        _recordingsPath = Path.Combine(Directory.GetCurrentDirectory(), "recordings");
        Directory.CreateDirectory(_recordingsPath);
    }
    
    public async Task<List<OnlineUser>> GetOnlineUsers()
    {
        return _userSessions.Values
            .Where(u => u.IsOnline && !u.UserId.StartsWith("viewer_"))
            .Select(u => new OnlineUser 
            { 
                UserId = u.UserId, 
                IsSharing = u.IsSharing,
                StartTime = u.StartTime
            })
            .ToList();
    }
    
    public string? GetUserConnection(string userId)
    {
        return _userSessions.Values
            .FirstOrDefault(u => u.UserId == userId && u.IsOnline)?.ConnectionId;
    }
    
    // Dicionário para mapear sharer -> viewer pendente
    private readonly ConcurrentDictionary<string, string> _pendingViewers = new();
    
    public void SetPendingViewer(string sharerConnectionId, string viewerConnectionId)
    {
        _pendingViewers.TryAdd(sharerConnectionId, viewerConnectionId);
    }
    
    public string? GetPendingViewer(string sharerConnectionId)
    {
        _pendingViewers.TryGetValue(sharerConnectionId, out var viewerConnectionId);
        return viewerConnectionId;
    }
    
    public void ClearPendingViewer(string sharerConnectionId)
    {
        _pendingViewers.TryRemove(sharerConnectionId, out _);
    }
    
    public async Task RegisterUser(string connectionId, string userId)
    {
        var userSession = new UserSession
        {
            ConnectionId = connectionId,
            UserId = userId,
            IsOnline = true,
            StartTime = DateTime.Now
        };
        
        _userSessions.AddOrUpdate(connectionId, userSession, (key, oldValue) => userSession);
    }
    
    public async Task HandleOffer(string connectionId, object offer)
    {
        var client = new ClientConnection
        {
            ConnectionId = connectionId,
            StartTime = DateTime.Now,
            IsRecording = true
        };
        
        _clients.TryAdd(connectionId, client);
        
        // Marcar usuário como compartilhando
        if (_userSessions.TryGetValue(connectionId, out var userSession))
        {
            userSession.IsSharing = true;
            
            // Iniciar gravação com ID do usuário
            _ = Task.Run(() => StartRecording(client, userSession.UserId));
        }
    }
    
    public async Task StopStream(string connectionId)
    {
        if (_clients.TryRemove(connectionId, out var client))
        {
            client.IsRecording = false;
            await StopRecording(client);
        }
        
        // Marcar usuário como não compartilhando
        if (_userSessions.TryGetValue(connectionId, out var userSession))
        {
            userSession.IsSharing = false;
        }
    }
    
    public async Task RemoveClient(string connectionId)
    {
        await StopStream(connectionId);
        
        // Remover usuário da sessão
        if (_userSessions.TryRemove(connectionId, out var userSession))
        {
            userSession.IsOnline = false;
        }
    }
    
    private async Task StartRecording(ClientConnection client, string userId)
    {
        try
        {
            var filename = $"recording_{userId}_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
            var outputPath = Path.Combine(_recordingsPath, filename);
            client.RecordingPath = outputPath;
            
            // Simular gravação (em uma implementação real, você capturaria o stream WebRTC)
            // Por enquanto, criamos um arquivo vazio que será preenchido quando o stream for recebido
            await File.WriteAllTextAsync(outputPath + ".info", $"User: {userId}\nRecording started at {client.StartTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar gravação: {ex.Message}");
        }
    }
    
    private async Task StopRecording(ClientConnection client)
    {
        try
        {
            if (!string.IsNullOrEmpty(client.RecordingPath))
            {
                // Finalizar gravação
                var infoFile = client.RecordingPath + ".info";
                if (File.Exists(infoFile))
                {
                    var info = await File.ReadAllTextAsync(infoFile);
                    info += $"\nRecording ended at {DateTime.Now}";
                    await File.WriteAllTextAsync(infoFile, info);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao parar gravação: {ex.Message}");
        }
    }
}

// Classe para representar uma conexão de cliente
public class ClientConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public bool IsRecording { get; set; }
    public string? RecordingPath { get; set; }
}

// Classe para representar uma sessão de usuário
public class UserSession
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public bool IsSharing { get; set; }
    public DateTime StartTime { get; set; }
}

// Classe para representar um usuário online
public class OnlineUser
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("isSharing")]
    public bool IsSharing { get; set; }
    
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }
}