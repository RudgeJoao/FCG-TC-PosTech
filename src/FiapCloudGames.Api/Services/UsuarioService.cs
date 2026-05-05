using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FiapCloudGames.Api.Data;
using FiapCloudGames.Api.Domain;
using FiapCloudGames.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Api.Services; 

public class UsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario> CriarUsuario(string nome, string email, string senha, PerfilUsuario perfil)
    {
        ValidarEmail(email);
        ValidarSenha(senha);

        var emailNormalizado = email.Trim().ToLower();
        var jaExiste = await _context.Usuarios.AnyAsync(x => x.Email == emailNormalizado);

        if (jaExiste)
            throw new ArgumentException("Ja existe usuario com este e-mail.");

        var hash = GerarHash(senha);
        var usuario = new Usuario(nome, emailNormalizado, hash, perfil);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Usuario> ValidarLogin(string email, string senha)
    {
        var emailNormalizado = email.Trim().ToLower();
        var hash = GerarHash(senha);

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Email == emailNormalizado && x.SenhaHash == hash);

        if (usuario == null)
            throw new UnauthorizedAccessException("E-mail ou senha invalidos.");

        return usuario;
    }

    public async Task<IReadOnlyList<Usuario>> ListarUsuarios()
    {
        return await _context.Usuarios
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Usuario> BuscarPorId(Guid id)
    {
        var usuario = await _context.Usuarios
            .Include(x => x.Biblioteca)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (usuario == null)
            throw new NotFoundException("Usuario nao encontrado.");

        return usuario;
    }

    public async Task AtualizarUsuario(Guid id, string nome, string email)
    {
        ValidarEmail(email);

        var usuario = await BuscarPorId(id);

        var emailNormalizado = email.Trim().ToLower();
        var emailEmUso = await _context.Usuarios
            .AnyAsync(x => x.Email == emailNormalizado && x.Id != id);

        if (emailEmUso)
            throw new ArgumentException("Ja existe outro usuario com este e-mail.");

        usuario.Nome = nome.Trim();
        usuario.Email = emailNormalizado;

        await _context.SaveChangesAsync();
    }

    public async Task RemoverUsuario(Guid id)
    {
        var usuario = await BuscarPorId(id);

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Jogo>> ListarBiblioteca(Guid usuarioId)
    {
        var usuario = await BuscarPorId(usuarioId);
        return usuario.Biblioteca;
    }

    public async Task AdicionarJogoNaBiblioteca(Guid usuarioId, Jogo jogo)
    {
        var usuario = await BuscarPorId(usuarioId);
        var jogoDoBanco = await _context.Jogos.FirstOrDefaultAsync(x => x.Id == jogo.Id);

        if (jogoDoBanco == null)
            throw new NotFoundException("Jogo nao encontrado.");

        usuario.AdicionarJogo(jogoDoBanco);
        await _context.SaveChangesAsync();
    }

    public async Task CriarAdminInicial()
    {
        var existeAdmin = await _context.Usuarios.AnyAsync(x => x.Email == "admin@fiap.com.br");

        if (!existeAdmin)
        {
            await CriarUsuario("Administrador", "admin@fiap.com.br", "Admin@123", PerfilUsuario.Administrador);
        }
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