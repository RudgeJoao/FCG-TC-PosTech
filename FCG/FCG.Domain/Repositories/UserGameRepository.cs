using FCG.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Domain.Repositories;

public interface IUserGameRepository
{
    Task<IEnumerable<UserGame>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserGame userGame, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
}

public class UserGameRepository
{
}
