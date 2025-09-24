using ScribeNest.Application.Interfaces;
using ScribeNest.Domain.Entities;
using ScribeNest.Infrastructure.Data;

namespace ScribeNest.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    private readonly AppDbContext _db = db;

    private IRepository<Post>? _posts;
    private IRepository<Category>? _categories;
    private IRepository<Comment>? _comments;

    public IRepository<Post> Posts => _posts ??= new RepositoryBase<Post>(_db);
    public IRepository<Category> Categories => _categories ??= new RepositoryBase<Category>(_db);
    public IRepository<Comment> Comments => _comments ??= new RepositoryBase<Comment>(_db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public void Dispose() => _db.Dispose();
}
