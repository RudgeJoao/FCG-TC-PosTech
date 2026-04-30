using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Domain.Entities;
public class UserGame
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public DateTime AcquiredAt { get; private set; }

    // Navegação
    public User User { get; private set; } = null!;
    public Game Game { get; private set; } = null!;

    // EF
    protected UserGame() { }

    public UserGame(User user, Game game)
    {
        Id = Guid.NewGuid();
        UserId = user.Id;
        GameId = game.Id;
        User = user;
        Game = game;
        AcquiredAt = DateTime.UtcNow;
    }
}