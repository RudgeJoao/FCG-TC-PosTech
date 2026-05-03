using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FiapCloudGames.Api.Domain;
using Microsoft.IdentityModel.Tokens;

namespace FiapCloudGames.Api.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString())
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(BuscarSecretKey()));
        var credencial = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credencial);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string BuscarSecretKey()
    {
        var secret = _configuration["Jwt:SecretKey"];

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Chave JWT nao configurada.");

        return secret;
    }
}
