using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FiapCloudGames.Api.Domain;

namespace FiapCloudGames.Api.Services;

public class TokenService
{
    private const string Segredo = "fiap-cloud-games-chave-local-de-estudo";

    public string GerarToken(Usuario usuario)
    {
        var header = new { alg = "HS256", typ = "JWT" };
        var payload = new
        {
            sub = usuario.Id,
            email = usuario.Email,
            role = usuario.Perfil.ToString(),
            exp = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds()
        };

        var parte1 = Base64Url(JsonSerializer.SerializeToUtf8Bytes(header));
        var parte2 = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        var assinatura = Assinar($"{parte1}.{parte2}");

        return $"{parte1}.{parte2}.{assinatura}";
    }

    public TokenUsuario? ValidarToken(string token)
    {
        var partes = token.Split('.');

        if (partes.Length != 3)
            return null;

        var assinaturaCerta = Assinar($"{partes[0]}.{partes[1]}");

        if (assinaturaCerta != partes[2])
            return null;

        var json = Encoding.UTF8.GetString(FromBase64Url(partes[1]));
        var payload = JsonSerializer.Deserialize<TokenPayload>(json);

        if (payload == null || payload.exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            return null;

        if (!Enum.TryParse<PerfilUsuario>(payload.role, out var perfil))
            return null;

        return new TokenUsuario(payload.sub, payload.email, perfil);
    }

    private static string Assinar(string texto)
    {
        var key = Encoding.UTF8.GetBytes(Segredo);
        var bytes = Encoding.UTF8.GetBytes(texto);

        using var hmac = new HMACSHA256(key);
        return Base64Url(hmac.ComputeHash(bytes));
    }

    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] FromBase64Url(string texto)
    {
        var base64 = texto.Replace('-', '+').Replace('_', '/');

        while (base64.Length % 4 != 0)
            base64 += "=";

        return Convert.FromBase64String(base64);
    }

    private class TokenPayload
    {
        public Guid sub { get; set; }
        public string email { get; set; } = "";
        public string role { get; set; } = "";
        public long exp { get; set; }
    }
}

public record TokenUsuario(Guid Id, string Email, PerfilUsuario Perfil);
