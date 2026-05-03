using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FiapCloudGames.Api.Domain;
using FiapCloudGames.Api.Services;
using Microsoft.Extensions.Configuration;

var testes = new List<(string Nome, Action Rodar)>
{
    ("Nao deixa criar e-mail invalido", EmailInvalido),
    ("Nao deixa senha fraca", SenhaFraca),
    ("Cria usuario com senha forte", CriarUsuario),
    ("Token gerado pode ser validado", TokenValido),
    ("Nao duplica jogo na biblioteca", BibliotecaSemDuplicar)
};

var falhas = 0;

foreach (var teste in testes)
{
    try
    {
        teste.Rodar();
        Console.WriteLine($"OK - {teste.Nome}");
    }
    catch (Exception ex)
    {
        falhas++;
        Console.WriteLine($"FALHOU - {teste.Nome}: {ex.Message}");
    }
}

if (falhas > 0)
{
    Environment.ExitCode = 1;
}

static void EmailInvalido()
{
    DeveDarErro(() => UsuarioService.ValidarEmail("email-errado"));
}

static void SenhaFraca()
{
    DeveDarErro(() => UsuarioService.ValidarSenha("123"));
}

static void CriarUsuario()
{
    var usuario = new Usuario("Ana", "ANA@EMAIL.COM", "hash-de-teste", PerfilUsuario.Usuario);

    Igual("ana@email.com", usuario.Email, "O e-mail deveria ser salvo em minusculo.");
}

static void TokenValido()
{
    var tokenService = new TokenService(CriarConfiguracao());
    var usuario = new Usuario("Bruno", "bruno@email.com", "hash-de-teste", PerfilUsuario.Usuario);

    var token = tokenService.GerarToken(usuario);
    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
    var usuarioId = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

    Igual(usuario.Id.ToString(), usuarioId, "O token deveria ter o id do usuario.");
}

static void BibliotecaSemDuplicar()
{
    var usuario = new Usuario("Carla", "carla@email.com", "hash-de-teste", PerfilUsuario.Usuario);
    var jogo = new Jogo("Banco de Dados", "Jogo sobre consultas SQL.", 10);

    usuario.AdicionarJogo(jogo);
    usuario.AdicionarJogo(jogo);

    Igual(1, usuario.Biblioteca.Count, "O jogo nao pode aparecer duas vezes.");
}

static void DeveDarErro(Action acao)
{
    try
    {
        acao();
    }
    catch (ArgumentException)
    {
        return;
    }

    throw new Exception("Era esperado erro de validacao.");
}

static void Igual<T>(T esperado, T recebido, string mensagem)
{
    if (!EqualityComparer<T>.Default.Equals(esperado, recebido))
    {
        throw new Exception($"{mensagem} Esperado: {esperado}. Recebido: {recebido}.");
    }
}

static IConfiguration CriarConfiguracao()
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
