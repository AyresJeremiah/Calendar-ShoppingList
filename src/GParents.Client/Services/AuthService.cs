using System.Net.Http.Json;
using GParents.Shared.DTOs;

namespace GParents.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(HttpClient http, CustomAuthStateProvider authStateProvider)
    {
        _http = http;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> CheckAccountExists()
    {
        var result = await _http.GetFromJsonAsync<AuthStatusResponse>("api/auth/status");
        return result?.AccountExists ?? false;
    }

    public async Task<(bool Success, string? Error)> Register(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new RegisterRequest
        {
            Username = username,
            Password = password
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null)
            {
                await _authStateProvider.MarkUserAsAuthenticated(result.Token);
                return (true, null);
            }
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task<(bool Success, string? Error)> Login(string username, string password, bool rememberMe)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest
        {
            Username = username,
            Password = password,
            RememberMe = rememberMe
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result != null)
            {
                await _authStateProvider.MarkUserAsAuthenticated(result.Token);
                return (true, null);
            }
        }

        return (false, "Invalid username or password");
    }

    public async Task Logout()
    {
        await _authStateProvider.MarkUserAsLoggedOut();
    }
}
