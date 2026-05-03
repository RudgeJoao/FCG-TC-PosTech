namespace FiapCloudGames.Api.Domain;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public PerfilUsuario Perfil { get; private set; }
    public List<Jogo> Biblioteca { get; private set; }

    public Usuario(string nome, string email, string senhaHash, PerfilUsuario perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome e obrigatorio.");

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Email = email.Trim().ToLower();
        SenhaHash = senhaHash;
        Perfil = perfil;
        Biblioteca = new List<Jogo>();
    }

    public void AdicionarJogo(Jogo jogo)
    {
        if (Biblioteca.Any(x => x.Id == jogo.Id))
            return;

        Biblioteca.Add(jogo);
    }
}
