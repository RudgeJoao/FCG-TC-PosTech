using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FiapCloudGames.Api.Domain;
using FiapCloudGames.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FiapCloudGames.Tests;

public class TokenTests
{
    [Fact]
    public void GerarToken_DeveColocarIdDoUsuarioNoJwt()
    {
        var tokenService = new TokenService(CriarConfiguracao());
        var usuario = new Usuario("Bruno", "bruno@email.com", "hash-de-teste", PerfilUsuario.Usuario);

        var token = tokenService.GerarToken(usuario);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var usuarioId = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        Assert.Equal(usuario.Id.ToString(), usuarioId);
    }

    private static IConfiguration CriarConfiguracao()
    {
        var dados = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "FiapCloudGames",
            ["Jwt:Audience"] = "FiapCloudGamesUsuarios",
            ["Jwt:SecretKey"] = "fiap-cloud-games-chave-jwt-local-para-estudo-2026"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dados)
            .Build();
    }
}
