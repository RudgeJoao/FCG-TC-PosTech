using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<UserGame> _library = [];
    public IReadOnlyCollection<UserGame> Library => _library.AsReadOnly();

    // Construtor para EF Core
    protected User() { }

    public User(string name, Email email, string passwordHash, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Hash da senha não pode ser vazio.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome não pode ser vazio.");

        Name = name.Trim();
    }

    public void UpdateEmail(Email email)
    {
        Email = email;
    }

    public void AddGameToLibrary(Game game)
    {
        if (_library.Any(g => g.GameId == game.Id))
            throw new DomainException($"O jogo '{game.Title}' já está na sua biblioteca.");

        _library.Add(new UserGame(this, game));
    }

    public bool IsAdmin() => Role == UserRole.Admin;
}

