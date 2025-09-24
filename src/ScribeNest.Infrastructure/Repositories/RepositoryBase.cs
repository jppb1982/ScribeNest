using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ScribeNest.Application.Interfaces;
using ScribeNest.Infrastructure.Data;

namespace ScribeNest.Infrastructure.Repositories;

public class RepositoryBase<T>(AppDbContext db) : IRepository<T> where T : class
{
    private readonly AppDbContext _db = db;

    public async Task<T?> GetByIdAsync(int id) => await _db.Set<T>().FindAsync(id);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null)
    {
        IQueryable<T> query = _db.Set<T>();
        if (predicate != null) query = query.Where(predicate);
        return await query.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(T entity) => await _db.Set<T>().AddAsync(entity);
    public void Update(T entity) => _db.Set<T>().Update(entity);
    public void Delete(T entity) => _db.Set<T>().Remove(entity);
}
