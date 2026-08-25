using Microsoft.EntityFrameworkCore;

namespace Dominion.API.Dominion.Persistance;

public class GameRepository
{
    private readonly DominionDbContext _db;

    public GameRepository(DominionDbContext db)
    {
        _db = db;
    }

    public async Task SaveAsync(
        Guid gameId,
        string mode,
        string stateJson,
        CancellationToken cancellationToken = default)
    {
        var game = await _db.Games
            .SingleOrDefaultAsync(
                g => g.Id == gameId,
                cancellationToken);

        if (game is null)
        {
            game = new GameEntity
            {
                Id = gameId,
                Mode = mode,
                StateJson = stateJson,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Games.Add(game);
        }
        else
        {
            game.Mode = mode;
            game.StateJson = stateJson;
            game.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<GameEntity?> LoadAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Games
            .Where(g => g.Id == gameId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Games
            .AnyAsync(g => g.Id == gameId, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var game = await _db.Games
            .SingleOrDefaultAsync(
                g => g.Id == gameId,
                cancellationToken);

        if (game is null)
            return;

        _db.Games.Remove(game);

        await _db.SaveChangesAsync(cancellationToken);
    }
}