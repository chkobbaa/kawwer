using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly KawwerDbContext _context;

    public UserRepository(KawwerDbContext context) => _context = context;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        var term = usernameOrEmail.Trim().ToLower();
        return _context.Users.FirstOrDefaultAsync(
            u => u.Username.ToLower() == term || u.Email.ToLower() == term,
            cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        var term = username.Trim().ToLower();
        return _context.Users.AnyAsync(u => u.Username.ToLower() == term, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var term = email.Trim().ToLower();
        return _context.Users.AnyAsync(u => u.Email.ToLower() == term, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> SearchAsync(string term, int maxResults, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{term.Trim()}%";
        return await _context.Users
            .Where(u => u.Status == AccountStatus.Active)
            .Where(u => EF.Functions.ILike(u.Username, pattern)
                        || EF.Functions.ILike(u.FirstName, pattern)
                        || EF.Functions.ILike(u.LastName, pattern))
            .OrderBy(u => u.Username)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<User>();
        }

        return await _context.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);
    }

    public void Add(User user) => _context.Users.Add(user);
}
