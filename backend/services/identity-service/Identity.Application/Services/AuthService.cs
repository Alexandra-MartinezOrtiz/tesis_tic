using System.Security.Claims;
using Identity.Application.Abstractions;
using Identity.Application.Dtos;
using Identity.Application.Options;
using Microsoft.Extensions.Options;

namespace Identity.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenIssuer _jwt;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUsuarioRepository usuarios,
        IPasswordHasher passwordHasher,
        IJwtTokenIssuer jwt,
        IOptions<JwtOptions> jwtOptions)
    {
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var full = await _usuarios.GetByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);
        if (full is null || !full.Activo)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var hash = await _usuarios.GetPasswordHashByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);
        if (hash is null || !_passwordHasher.Verify(request.Password, hash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        return await IssueTokensAsync(full, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var principal = _jwt.ValidateRefreshToken(request.RefreshToken);
        if (principal is null)
            throw new UnauthorizedAccessException("Refresh token inválido.");

        var jti = principal.FindFirst("jti")?.Value;
        if (string.IsNullOrEmpty(jti) || !Guid.TryParse(jti, out var jtiGuid))
            throw new UnauthorizedAccessException("Refresh token inválido.");

        if (!await _usuarios.RefreshTokenIsActiveAsync(jtiGuid, cancellationToken).ConfigureAwait(false))
            throw new UnauthorizedAccessException("Refresh token revocado o expirado.");

        var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(sub) || !long.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("Refresh token inválido.");

        var full = await _usuarios.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (full is null || !full.Activo)
            throw new UnauthorizedAccessException("Usuario no disponible.");

        await _usuarios.RevokeRefreshTokenAsync(jtiGuid, cancellationToken).ConfigureAwait(false);
        return await IssueTokensAsync(full, cancellationToken).ConfigureAwait(false);
    }

    public Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        _ = request;
        return Task.CompletedTask;
    }

    public Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        _ = request;
        return Task.CompletedTask;
    }

    private async Task<TokenResponse> IssueTokensAsync(UsuarioDetalleDto user, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.Nombres} {user.Apellidos}".Trim()),
        };
        foreach (var r in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, r.Nombre));

        var access = _jwt.CreateAccessToken(claims);
        var refresh = _jwt.CreateRefreshToken(user.Id, out var jti);
        var expira = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        await _usuarios.StoreRefreshTokenAsync(user.Id, jti, DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays), cancellationToken)
            .ConfigureAwait(false);
        return new TokenResponse(access, refresh, expira);
    }
}
