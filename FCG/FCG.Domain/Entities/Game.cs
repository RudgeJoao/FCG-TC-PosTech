using FCG.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Domain.Entities;
public class Game
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string Genre { get; private set; }
    public DateTime ReleasedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Construtor para EF Core
    protected Game() { }

    public Game(string title, string description, decimal price, string genre, DateTime releasedAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título do jogo não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição do jogo não pode ser vazia.");

        if (price < 0)
            throw new DomainException("Preço não pode ser negativo.");

        if (string.IsNullOrWhiteSpace(genre))
            throw new DomainException("Gênero do jogo não pode ser vazio.");

        Id = Guid.NewGuid();
        Title = title.Trim();
        Description = description.Trim();
        Price = price;
        Genre = genre.Trim();
        ReleasedAt = releasedAt;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, decimal price, string genre, DateTime releasedAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título do jogo não pode ser vazio.");

        if (price < 0)
            throw new DomainException("Preço não pode ser negativo.");

        Title = title.Trim();
        Description = description.Trim();
        Price = price;
        Genre = genre.Trim();
        ReleasedAt = releasedAt;
    }
}
