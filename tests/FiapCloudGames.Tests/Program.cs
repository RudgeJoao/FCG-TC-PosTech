using FiapCloudGames.Api.Domain;
using FiapCloudGames.Api.Services;

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
    var service = new UsuarioService();

    DeveDarErro(() => service.CriarUsuario("Ana", "email-errado", "Senha@123", PerfilUsuario.Usuario));
}

static void SenhaFraca()
{
    var service = new UsuarioService();

    DeveDarErro(() => service.CriarUsuario("Ana", "ana@email.com", "123", PerfilUsuario.Usuario));
}

static void CriarUsuario()
{
    var service = new UsuarioService();
    var usuario = service.CriarUsuario("Ana", "ana@email.com", "Senha@123", PerfilUsuario.Usuario);

    Igual("ana@email.com", usuario.Email, "O e-mail deveria ser salvo em minusculo.");
}

static void TokenValido()
{
    var usuarioService = new UsuarioService();
    var tokenService = new TokenService();
    var usuario = usuarioService.CriarUsuario("Bruno", "bruno@email.com", "Senha@123", PerfilUsuario.Usuario);

    var token = tokenService.GerarToken(usuario);
    var usuarioDoToken = tokenService.ValidarToken(token);

    Igual(usuario.Id, usuarioDoToken?.Id, "O token deveria ter o id do usuario.");
}

static void BibliotecaSemDuplicar()
{
    var usuarioService = new UsuarioService();
    var jogoService = new JogoService();

    var usuario = usuarioService.CriarUsuario("Carla", "carla@email.com", "Senha@123", PerfilUsuario.Usuario);
    var jogo = jogoService.CriarJogo("Banco de Dados", "Jogo sobre consultas SQL.", 10);

    usuarioService.AdicionarJogoNaBiblioteca(usuario.Id, jogo);
    usuarioService.AdicionarJogoNaBiblioteca(usuario.Id, jogo);

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
