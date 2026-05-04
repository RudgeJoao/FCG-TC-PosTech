using FiapCloudGames.Api.Data;
using FiapCloudGames.Api.Domain;
using FiapCloudGames.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Api.Services;

public class JogoService
{
    private readonly AppDbContext _context;

    public JogoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Jogo> CriarJogo(string nome, string descricao, decimal preco)
    {
        var jogo = new Jogo(nome, descricao, preco);

        _context.Jogos.Add(jogo);
        await _context.SaveChangesAsync();

        return jogo;
    }

    public async Task<IReadOnlyList<Jogo>> ListarJogos()
    {
        return await _context.Jogos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Jogo> BuscarPorId(Guid id)
    {
        var jogo = await _context.Jogos.FirstOrDefaultAsync(x => x.Id == id);

        if (jogo == null)
            throw new NotFoundException("Jogo nao encontrado.");

        return jogo;
    }

    public async Task AtualizarJogo(Guid id, string nome, string descricao, decimal preco)
    {
        var jogo = await BuscarPorId(id);

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do jogo e obrigatorio.");

        if (preco < 0)
            throw new ArgumentException("Preco nao pode ser negativo.");

        jogo.Nome = nome.Trim();
        jogo.Descricao = descricao.Trim();
        jogo.Preco = preco;

        await _context.SaveChangesAsync();
    }

    public async Task RemoverJogo(Guid id)
    {
        var jogo = await BuscarPorId(id);

        _context.Jogos.Remove(jogo);
        await _context.SaveChangesAsync();
    }

    public async Task CriarJogosIniciais()
    {
        var temJogos = await _context.Jogos.AnyAsync();

        if (!temJogos)
        {
            await CriarJogo("C# para Iniciantes", "Jogo educativo sobre logica e C#.", 49.90m);
            await CriarJogo("API Quest", "Jogo para treinar verbos HTTP e endpoints.", 29.90m);
        }
    }
}