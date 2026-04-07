using System.Security.Claims;

namespace Identity.Application.Abstractions;

public interface IJwtTokenIssuer
{
    string CreateAccessToken(IEnumerable<Claim> claims);
    string CreateRefreshToken(long userId, out Guid jti);
    ClaimsPrincipal? ValidateRefreshToken(string token);
}
