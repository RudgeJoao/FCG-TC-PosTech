namespace FiapCloudGames.Api.Domain;

public class Jogo
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public decimal Preco { get; set; }

    public Jogo()
    {
    }

    public Jogo(string nome, string descricao, decimal preco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do jogo e obrigatorio.");

        if (preco < 0)
            throw new ArgumentException("Preco nao pode ser negativo.");

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        Descricao = descricao.Trim();
        Preco = preco;
    }
}
