using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FiapCloudGames.Api.Domain;

namespace FiapCloudGames.Api.Services;

public class UsuarioService
{
    private readonly List<Usuario> _usuarios = new();

    public UsuarioService()
    {
        CriarUsuario("Administrador", "admin@fiap.com.br", "Admin@123", PerfilUsuario.Administrador);
    }

    public Usuario CriarUsuario(string nome, string email, string senha, PerfilUsuario perfil)
    {
        ValidarEmail(email);
        ValidarSenha(senha);

        if (_usuarios.Any(x => x.Email == email.Trim().ToLower()))
            throw new ArgumentException("Ja existe usuario com este e-mail.");

        var hash = GerarHash(senha);
        var usuario = new Usuario(nome, email, hash, perfil);

        _usuarios.Add(usuario);

        return usuario;
    }

    public Usuario ValidarLogin(string email, string senha)
    {
        var emailNormalizado = email.Trim().ToLower();
        var hash = GerarHash(senha);

        var usuario = _usuarios.FirstOrDefault(x => x.Email == emailNormalizado && x.SenhaHash == hash);

        if (usuario == null)
            throw new UnauthorizedAccessException("E-mail ou senha invalidos.");

        return usuario;
    }

    public IReadOnlyList<Usuario> ListarUsuarios()
    {
        return _usuarios;
    }

    public Usuario BuscarPorId(Guid id)
    {
        var usuario = _usuarios.FirstOrDefault(x => x.Id == id);

        if (usuario == null)
            throw new ArgumentException("Usuario nao encontrado.");

        return usuario;
    }

    public IReadOnlyList<Jogo> ListarBiblioteca(Guid usuarioId)
    {
        return BuscarPorId(usuarioId).Biblioteca;
    }

    public void AdicionarJogoNaBiblioteca(Guid usuarioId, Jogo jogo)
    {
        var usuario = BuscarPorId(usuarioId);
        usuario.AdicionarJogo(jogo);
    }

    public static void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-mail e obrigatorio.");

        var ok = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        if (!ok)
            throw new ArgumentException("Formato de e-mail invalido.");
    }

    public static void ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
            throw new ArgumentException("Senha precisa ter pelo menos 8 caracteres.");

        var temLetra = senha.Any(char.IsLetter);
        var temNumero = senha.Any(char.IsDigit);
        var temEspecial = senha.Any(x => !char.IsLetterOrDigit(x));

        if (!temLetra || !temNumero || !temEspecial)
            throw new ArgumentException("Senha precisa ter letras, numeros e caracteres especiais.");
    }

    private static string GerarHash(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes);
    }
}
