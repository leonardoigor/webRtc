using Microsoft.AspNetCore.SignalR;
using SIPSorcery.Net;
using SIPSorcery.Media;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using FFMpegCore;
using FFMpegCore.Pipes;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Application.Interfaces;
using WebRtcServer.Application.Services;
using WebRtcServer.Domain.Entities;
using WebRtcServer.Domain.Enums;
using WebRtcServer.Domain.Interfaces;
using WebRtcServer.Domain.ValueObjects;
using WebRtcServer.Infrastructure.Repositories;
using WebRtcServer.Presentation.Hubs;
using WebRtcServer.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddControllers();
builder.Services.AddSignalR(options =>
{
    // Configurações de timeout ajustadas para evitar desconexões prematuras
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(120); // Aumentado para 2 minutos
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);       // Aumentado para 30 segundos
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);       // Aumentado para 30 segundos
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.StreamBufferCapacity = 10;
    
    // Configurações adicionais para estabilidade
    options.EnableDetailedErrors = true; // Para debugging
});

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "WebRTC Server API",
        Version = "v1",
        Description = "API para gerenciamento de sessões WebRTC, usuários, streaming e gravações",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "WebRTC Server",
            Email = "support@webrtcserver.com"
        }
    });
    
    // Incluir comentários XML para documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Registrar repositórios
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ISessionRepository, SessionRepository>();
builder.Services.AddSingleton<IRecordingRepository, RecordingRepository>();
builder.Services.AddSingleton<IConnectionRepository, ConnectionRepository>();

// Registrar serviços de aplicação
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStreamingService, StreamingService>();
builder.Services.AddScoped<IRecordingApplicationService, RecordingApplicationService>();

// Registrar serviços de domínio (implementações temporárias)
builder.Services.AddScoped<IWebRtcService, WebRtcService>();
builder.Services.AddScoped<IRecordingService, RecordingService>();

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
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebRTC Server API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "WebRTC Server API Documentation";
});

app.UseCors();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<WebRtcHub>("/webrtchub");

// Endpoint de status do servidor
app.MapGet("/", () => Results.Json(new { 
    status = "WebRTC Server Running", 
    timestamp = DateTime.Now,
    endpoints = new[] { "/webrtchub", "/api/recordings", "/recordings/{filename}", "/api/register-desktop" }
}));

// API para registro de clientes desktop
app.MapPost("/api/desktop/register", async (DesktopRegistrationRequest request, IUserService userService) =>
{
    try
    {
        Console.WriteLine($"=== API DESKTOP REGISTER ===");
        Console.WriteLine($"Client ID: {request.ClientId}");
        Console.WriteLine($"Group ID: {request.GroupId}");
        
        // Gerar um connection ID temporário para o cliente desktop
        var connectionId = Guid.NewGuid().ToString();
        // Usar apenas os primeiros 8 caracteres dos GUIDs para manter o userId dentro do limite de 50 caracteres
        var clientIdShort = request.ClientId.Replace("-", "")[..8];
        var groupIdShort = request.GroupId.Replace("-", "")[..8];
        var userId = $"desktop_{clientIdShort}_{groupIdShort}";
        
        // Registrar o cliente desktop
        var userDto = await userService.RegisterUserAsync(new CreateUserDto
        {
            UserId = userId,
            ConnectionId = connectionId,
            Type = UserType.DesktopClient,
            GroupId = request.GroupId
        });
        
        await userService.SetUserOnlineAsync(userDto.UserId, connectionId);
        
        var response = new DesktopRegistrationResponse
        {
            Success = true,
            ConnectionId = connectionId,
            Message = "Cliente desktop registrado com sucesso"
        };
        
        Console.WriteLine($"✅ Resposta: {System.Text.Json.JsonSerializer.Serialize(response)}");
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro no registro: {ex.Message}");
        return Results.BadRequest(new DesktopRegistrationResponse
        {
            Success = false,
            ConnectionId = string.Empty,
            Message = $"Erro ao registrar cliente: {ex.Message}"
        });
    }
});

// Endpoint para receber ofertas WebRTC de clientes desktop
app.MapPost("/api/desktop/offer", async (DesktopOfferRequest request, IWebRtcService webRtcService, IUserService userService, IStreamingService streamingService) =>
{
    try
    {
        var user = await userService.GetUserByConnectionIdAsync(request.ConnectionId);
        if (user != null)
        {
            var sessionDto = await streamingService.StartSessionAsync(user.Id);
            await streamingService.StartSharingAsync(sessionDto.Id);
            
            await webRtcService.CreateOfferAsync(request.ConnectionId, request.Offer.ToString() ?? "");
        }
        
        return Results.Json(new DesktopOfferResponse
        {
            Success = true,
            Message = "Offer processed successfully"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new DesktopOfferResponse
        {
            Success = false,
            Message = $"Offer processing failed: {ex.Message}"
        });
    }
});

// Endpoint para receber candidatos ICE de clientes desktop
app.MapPost("/api/desktop/ice-candidate", async (DesktopIceCandidateRequest request, IWebRtcService webRtcService) =>
{
    try
    {
        // Converter o objeto candidato para JSON para preservar sua estrutura
        string candidateJson = System.Text.Json.JsonSerializer.Serialize(request.Candidate);
        await webRtcService.AddIceCandidateAsync(request.ConnectionId, candidateJson);
        
        return Results.Json(new DesktopIceCandidateResponse
        {
            Success = true,
            Message = "ICE candidate processed successfully"
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new DesktopIceCandidateResponse
        {
            Success = false,
            Message = $"ICE candidate processing failed: {ex.Message}"
        });
    }
});
 
// API para listar gravações
app.MapGet("/api/recordings", async (IRecordingApplicationService recordingService) =>
{
    try
    {
        var recordings = await recordingService.GetAllRecordingsAsync();
        return Results.Json(new { recordings });
    }
    catch (Exception ex)
    {
        return Results.Json(new { recordings = new List<object>(), error = ex.Message });
    }
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





// Classes para registro de clientes desktop
public class DesktopRegistrationRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
}

public class DesktopRegistrationResponse
{
    public bool Success { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

// Classes para ofertas WebRTC de desktop
public class DesktopOfferRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public object Offer { get; set; } = new();
}

public class DesktopOfferResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

// Classes para candidatos ICE de desktop
public class DesktopIceCandidateRequest
{
    public string ConnectionId { get; set; } = string.Empty;
    public object Candidate { get; set; } = new();
}

public class DesktopIceCandidateResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}