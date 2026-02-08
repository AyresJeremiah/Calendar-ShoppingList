using GParents.Server.Services;
using GParents.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GParents.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly TokenService _tokenService;

    public AuthController(AuthService authService, TokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<AuthStatusResponse>> GetStatus()
    {
        return Ok(new AuthStatusResponse { AccountExists = await _authService.AccountExistsAsync() });
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _authService.AccountExistsAsync())
            return Conflict("Account already exists");

        var user = await _authService.RegisterAsync(request.Username, request.Password);
        if (user == null)
            return Conflict("Account already exists");

        var token = _tokenService.GenerateToken(user.Id, user.Username, true);
        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            ExpiresInDays = 30
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _authService.LoginAsync(request.Username, request.Password);
        if (user == null)
            return Unauthorized("Invalid username or password");

        var token = _tokenService.GenerateToken(user.Id, user.Username, request.RememberMe);
        return Ok(new AuthResponse
        {
            Token = token,
            Username = user.Username,
            ExpiresInDays = request.RememberMe ? 30 : 1
        });
    }
}
