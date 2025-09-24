using ScribeNest.Domain.Entities;

namespace ScribeNest.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Post> Posts { get; }
    IRepository<Category> Categories { get; }
    IRepository<Comment> Comments { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
