namespace SmartReceipt.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Email, 
    string Password, 
    string FirstName, 
    string LastName);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User);

public record RefreshTokenRequest(string RefreshToken);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName);
