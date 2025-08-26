using Microsoft.AspNetCore.Mvc;
using WebRtcServer.Application.DTOs;
using WebRtcServer.Application.Interfaces;
using WebRtcServer.Domain.Enums;

namespace WebRtcServer.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    /// <param name="createUserDto">Dados do usuário a ser criado</param>
    /// <returns>Dados do usuário criado</returns>
    [HttpPost]
    public async Task<ActionResult<UserDto>> RegisterUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var userDto = await _userService.RegisterUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { userId = userDto.UserId }, userDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um usuário pelo ID
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Dados do usuário</returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(string userId)
    {
        try
        {
            var user = await _userService.GetUserByUserIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Obtém todos os usuários online
    /// </summary>
    /// <returns>Lista de usuários online</returns>
    [HttpGet("online")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetOnlineUsers()
    {
        try
        {
            var users = await _userService.GetOnlineUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Define um usuário como online
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="connectionId">ID da conexão</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{userId}/online")]
    public async Task<ActionResult> SetUserOnline(string userId, [FromBody] string connectionId)
    {
        try
        {
            await _userService.SetUserOnlineAsync(userId, connectionId);
            return Ok(new { message = "Usuário definido como online" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Define um usuário como offline
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{userId}/offline")]
    public async Task<ActionResult> SetUserOffline(string userId)
    {
        try
        {
            await _userService.SetUserOfflineAsync(userId);
            return Ok(new { message = "Usuário definido como offline" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


}