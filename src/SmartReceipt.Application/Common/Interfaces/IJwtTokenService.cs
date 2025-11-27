using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateToken(string token);
}