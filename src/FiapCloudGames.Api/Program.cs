using System.Security.Claims;
using System.Text;
using FiapCloudGames.Api.Data;
using FiapCloudGames.Api.Domain;
using FiapCloudGames.Api.Exceptions;
using FiapCloudGames.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

#region Builder

var builder = WebApplication.CreateBuilder(args);

#endregion

#region Logging

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FIAP Cloud Games API",
        Version = "v1",
        Description = "API da fase 1 para usuarios, jogos e biblioteca."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole apenas o token JWT gerado no login, sem escrever Bearer antes."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#region Autenticacao e Autorizacao

var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    erro = "Voce precisa fazer login com um token valido."
                });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    erro = "Somente usuarios administradores podem realizar essa acao."
                });
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole(PerfilUsuario.Administrador.ToString()));
});

#endregion

#region Banco de Dados

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Injecao de Dependencia

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<JogoService>();
builder.Services.AddSingleton<TokenService>();

#endregion

var app = builder.Build();

#region Seed do Banco

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var usuarios = scope.ServiceProvider.GetRequiredService<UsuarioService>();
    var jogos = scope.ServiceProvider.GetRequiredService<JogoService>();

    await usuarios.CriarAdminInicial();
    await jogos.CriarJogosIniciais();
}

#endregion

#region Pipeline

app.UseMiddleware<ErroMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Endpoints - Geral

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

#endregion

#region Endpoints - Autenticacao

app.MapPost("/login", async (LoginRequest request, UsuarioService usuarioService, TokenService tokenService) =>
{
    var usuario = await usuarioService.ValidarLogin(request.Email, request.Senha);
    var token = tokenService.GerarToken(usuario);

    return Results.Ok(new LoginResponse(token, usuario.ToResponse()));
})
.WithTags("Autenticacao")
.WithSummary("Fazer login");

#endregion

#region Endpoints - Usuarios

app.MapPost("/usuarios", async (CriarUsuarioRequest request, UsuarioService service) =>
{
    var usuario = await service.CriarUsuario(request.Nome, request.Email, request.Senha, PerfilUsuario.Usuario);
    return Results.Created($"/usuarios/{usuario.Id}", usuario.ToResponse());
})
.WithTags("Usuarios")
.WithSummary("Cadastrar usuario");

app.MapGet("/usuarios", async (HttpContext context, UsuarioService service) =>
{
    var erro = context.ValidarAdmin();
    if (erro != null) return erro;

    var usuarios = await service.ListarUsuarios();
    return Results.Ok(usuarios.Select(x => x.ToResponse()));
})
.WithTags("Usuarios")
.WithSummary("Listar usuarios (Admin)")
.RequireAuthorization();

app.MapPut("/usuarios/{id:guid}", async (Guid id, AtualizarUsuarioRequest request, HttpContext context, UsuarioService service) =>
{
    var erro = context.ValidarAdmin();
    if (erro != null) return erro;

    await service.AtualizarUsuario(id, request.Nome, request.Email);
    return Results.NoContent();
})
.WithTags("Usuarios")
.WithSummary("Atualizar usuario (Admin)")
.RequireAuthorization();

app.MapDelete("/usuarios/{id:guid}", async (Guid id, HttpContext context, UsuarioService service) =>
{
    var erro = context.ValidarAdmin();
    if (erro != null) return erro;

    await service.RemoverUsuario(id);
    return Results.NoContent();
})
.WithTags("Usuarios")
.WithSummary("Remover usuario (Admin)")
.RequireAuthorization();

#endregion

#region Endpoints - Jogos

app.MapGet("/jogos", async (JogoService service) =>
{
    var jogos = await service.ListarJogos();
    return Results.Ok(jogos.Select(x => x.ToResponse()));
})
.WithTags("Jogos")
.WithSummary("Listar jogos")
.RequireAuthorization();

app.MapPost("/jogos", async (CriarJogoRequest request, HttpContext context, JogoService service) =>
{
    var erro = context.ValidarAdmin();
    if (erro != null) return erro;

    var jogo = await service.CriarJogo(request.Nome, request.Descricao, request.Preco);
    return Results.Created($"/jogos/{jogo.Id}", jogo.ToResponse());
})
.WithTags("Jogos")
.WithSummary("Cadastrar jogo (Admin)")
.RequireAuthorization();

app.MapPut("/jogos/{id:guid}", async (Guid id, AtualizarJogoRequest request, HttpContext context, JogoService service) =>
{
    var erro = context.ValidarAdmin();
    if (erro != null) return erro;

    await service.AtualizarJogo(id, request.Nome, request.Descricao, request.Preco);
    return Results.NoContent();
})
.WithTags("Jogos")
.WithSummary("Atualizar jogo (Admin)")
.RequireAuthorization();

app.MapDelete("/jogos/{id:guid}", async (Guid id, HttpContext context, JogoService service) =>
{
    var erro = context.ValidarAdmin();
    if (erro != null) return erro;

    await service.RemoverJogo(id);
    return Results.NoContent();
})
.WithTags("Jogos")
.WithSummary("Remover jogo (Admin)")
.RequireAuthorization();

#endregion

#region Endpoints - Biblioteca

app.MapGet("/usuarios/{id:guid}/biblioteca", async (Guid id, HttpContext context, UsuarioService service) =>
{
    var perfil = context.User.FindFirstValue(ClaimTypes.Role);
    var usuarioId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (perfil != PerfilUsuario.Administrador.ToString() && usuarioId != id.ToString())
        return Results.Forbid();

    var jogos = await service.ListarBiblioteca(id);
    return Results.Ok(jogos.Select(x => x.ToResponse()));
})
.WithTags("Biblioteca")
.WithSummary("Listar biblioteca do usuario")
.RequireAuthorization();

app.MapPost("/usuarios/{usuarioId:guid}/jogos/{jogoId:guid}", async (
    Guid usuarioId,
    Guid jogoId,
    HttpContext context,
    UsuarioService usuarios,
    JogoService jogos) =>
{
    var perfil = context.User.FindFirstValue(ClaimTypes.Role);
    var usuarioLogado = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (perfil != PerfilUsuario.Administrador.ToString() && usuarioLogado != usuarioId.ToString())
        return Results.Forbid();

    var jogo = await jogos.BuscarPorId(jogoId);
    await usuarios.AdicionarJogoNaBiblioteca(usuarioId, jogo);

    return Results.Ok(new { mensagem = "Jogo adicionado na biblioteca" });
})
.WithTags("Biblioteca")
.WithSummary("Adicionar jogo na biblioteca")
.RequireAuthorization();

#endregion

app.Run();

public partial class Program { }

#region Records - Requests e Responses

public record CriarUsuarioRequest(string Nome, string Email, string Senha);
public record AtualizarUsuarioRequest(string Nome, string Email);
public record LoginRequest(string Email, string Senha);
public record CriarJogoRequest(string Nome, string Descricao, decimal Preco);
public record AtualizarJogoRequest(string Nome, string Descricao, decimal Preco);
public record LoginResponse(string Token, UsuarioResponse Usuario);
public record UsuarioResponse(Guid Id, string Nome, string Email, string Perfil);
public record JogoResponse(Guid Id, string Nome, string Descricao, decimal Preco);

#endregion

#region Extensions

static class ResponseExtensions
{
    public static UsuarioResponse ToResponse(this Usuario usuario)
        => new(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil.ToString());

    public static JogoResponse ToResponse(this Jogo jogo)
        => new(jogo.Id, jogo.Nome, jogo.Descricao, jogo.Preco);
}

static class PermissaoExtensions
{
    public static IResult? ValidarAdmin(this HttpContext context)
    {
        var perfil = context.User.FindFirstValue(ClaimTypes.Role);

        if (perfil == PerfilUsuario.Administrador.ToString())
            return null;

        return Results.Json(new
        {
            erro = "Somente usuarios administradores podem realizar essa acao."
        }, statusCode: 403);
    }
}

#endregion

#region Middleware

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
        catch (NotFoundException ex)
        {
            await EscreverErro(context, 404, ex.Message);
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

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

#endregion