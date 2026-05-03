using FiapCloudGames.Api.Domain;

namespace FiapCloudGames.Api.Services;

public class JogoService
{
    private readonly List<Jogo> _jogos = new();

    public JogoService()
    {
        CriarJogo("C# para Iniciantes", "Jogo educativo sobre logica e C#.", 49.90m);
        CriarJogo("API Quest", "Jogo para treinar verbos HTTP e endpoints.", 29.90m);
    }

    public Jogo CriarJogo(string nome, string descricao, decimal preco)
    {
        var jogo = new Jogo(nome, descricao, preco);
        _jogos.Add(jogo);

        return jogo;
    }

    public IReadOnlyList<Jogo> ListarJogos()
    {
        return _jogos;
    }

    public Jogo BuscarPorId(Guid id)
    {
        var jogo = _jogos.FirstOrDefault(x => x.Id == id);

        if (jogo == null)
            throw new ArgumentException("Jogo nao encontrado.");

        return jogo;
    }
}
