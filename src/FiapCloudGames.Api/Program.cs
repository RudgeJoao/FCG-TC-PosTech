using System.Security.Claims;
using System.Text.Json;
using FiapCloudGames.Api.Domain;
using FiapCloudGames.Api.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FIAP Cloud Games API",
        Version = "v1",
        Description = "API da fase 1 para usuarios, jogos e biblioteca."
    });
});

builder.Services.AddSingleton<UsuarioService>();
builder.Services.AddSingleton<JogoService>();
builder.Services.AddSingleton<TokenService>();

var app = builder.Build();

app.UseMiddleware<ErroMiddleware>();
app.UseMiddleware<TokenMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapPost("/usuarios", (CriarUsuarioRequest request, UsuarioService service) =>
{
    var usuario = service.CriarUsuario(request.Nome, request.Email, request.Senha, PerfilUsuario.Usuario);
    return Results.Created($"/usuarios/{usuario.Id}", usuario.ToResponse());
})
.WithTags("Usuarios")
.WithSummary("Cadastrar usuario");

app.MapPost("/login", (LoginRequest request, UsuarioService usuarioService, TokenService tokenService) =>
{
    var usuario = usuarioService.ValidarLogin(request.Email, request.Senha);
    var token = tokenService.GerarToken(usuario);

    return Results.Ok(new LoginResponse(token, usuario.ToResponse()));
})
.WithTags("Autenticacao")
.WithSummary("Fazer login");

app.MapGet("/usuarios", (HttpContext context, UsuarioService service) =>
{
    context.PrecisaSerAdmin();

    return Results.Ok(service.ListarUsuarios().Select(x => x.ToResponse()));
})
.WithTags("Usuarios")
.WithSummary("Listar usuarios");

app.MapGet("/usuarios/{id:guid}/biblioteca", (Guid id, HttpContext context, UsuarioService service) =>
{
    context.PrecisaEstarLogado();
    var perfil = context.User.FindFirstValue(ClaimTypes.Role);
    var usuarioId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (perfil != PerfilUsuario.Administrador.ToString() && usuarioId != id.ToString())
    {
        return Results.Forbid();
    }

    var jogos = service.ListarBiblioteca(id);
    return Results.Ok(jogos.Select(x => x.ToResponse()));
})
.WithTags("Biblioteca")
.WithSummary("Listar biblioteca do usuario");

app.MapGet("/jogos", (HttpContext context, JogoService service) =>
{
    context.PrecisaEstarLogado();
    return Results.Ok(service.ListarJogos().Select(x => x.ToResponse()));
})
.WithTags("Jogos")
.WithSummary("Listar jogos");

app.MapPost("/jogos", (CriarJogoRequest request, HttpContext context, JogoService service) =>
{
    context.PrecisaSerAdmin();

    var jogo = service.CriarJogo(request.Nome, request.Descricao, request.Preco);
    return Results.Created($"/jogos/{jogo.Id}", jogo.ToResponse());
})
.WithTags("Jogos")
.WithSummary("Cadastrar jogo");

app.MapPost("/usuarios/{usuarioId:guid}/jogos/{jogoId:guid}", (
    Guid usuarioId,
    Guid jogoId,
    HttpContext context,
    UsuarioService usuarios,
    JogoService jogos) =>
{
    context.PrecisaEstarLogado();

    var perfil = context.User.FindFirstValue(ClaimTypes.Role);
    var usuarioLogado = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (perfil != PerfilUsuario.Administrador.ToString() && usuarioLogado != usuarioId.ToString())
    {
        return Results.Forbid();
    }

    var jogo = jogos.BuscarPorId(jogoId);
    usuarios.AdicionarJogoNaBiblioteca(usuarioId, jogo);

    return Results.Ok(new { mensagem = "Jogo adicionado na biblioteca" });
})
.WithTags("Biblioteca")
.WithSummary("Adicionar jogo na biblioteca");

app.Run();

public partial class Program { }

public record CriarUsuarioRequest(string Nome, string Email, string Senha);
public record LoginRequest(string Email, string Senha);
public record CriarJogoRequest(string Nome, string Descricao, decimal Preco);
public record LoginResponse(string Token, UsuarioResponse Usuario);
public record UsuarioResponse(Guid Id, string Nome, string Email, string Perfil);
public record JogoResponse(Guid Id, string Nome, string Descricao, decimal Preco);

static class ResponseExtensions
{
    public static UsuarioResponse ToResponse(this Usuario usuario)
    {
        return new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil.ToString());
    }

    public static JogoResponse ToResponse(this Jogo jogo)
    {
        return new JogoResponse(jogo.Id, jogo.Nome, jogo.Descricao, jogo.Preco);
    }
}

static class HttpContextExtensions
{
    public static void PrecisaEstarLogado(this HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Voce precisa fazer login primeiro.");
        }
    }

    public static void PrecisaSerAdmin(this HttpContext context)
    {
        context.PrecisaEstarLogado();

        var perfil = context.User.FindFirstValue(ClaimTypes.Role);
        if (perfil != PerfilUsuario.Administrador.ToString())
        {
            throw new ForbiddenException("Apenas administrador pode acessar esta rota.");
        }
    }
}

public class ErroMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErroMiddleware> _logger;

    public ErroMiddleware(RequestDelegate next, ILogger<ErroMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            await EscreverErro(context, 401, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await EscreverErro(context, 403, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await EscreverErro(context, 400, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro nao tratado");
            await EscreverErro(context, 500, "Erro interno na API.");
        }
    }

    private static async Task EscreverErro(HttpContext context, int status, string mensagem)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { erro = mensagem });
    }
}

public class TokenMiddleware
{
    private readonly RequestDelegate _next;

    public TokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TokenService tokenService)
    {
        var authorization = context.Request.Headers.Authorization.ToString();

        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authorization["Bearer ".Length..].Trim();
            var usuario = tokenService.ValidarToken(token);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new(ClaimTypes.Email, usuario.Email),
                    new(ClaimTypes.Role, usuario.Perfil.ToString())
                };

                var identity = new ClaimsIdentity(claims, "jwt-simples");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
