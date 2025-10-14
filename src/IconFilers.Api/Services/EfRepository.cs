using IconFilers.Api.IServices;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IconFilers.Api.Services
{
    public class EfRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _db;

        public EfRepository(AppDbContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        {
            await _db.Set<T>().AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        {
            var q = _db.Set<T>().AsQueryable();
            if (predicate != null) q = q.Where(predicate);
            return await q.CountAsync(ct);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _db.Set<T>().Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public virtual async Task<IList<T>> FindAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            CancellationToken ct = default,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> q = _db.Set<T>();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                    q = q.Include(include);
            }

            if (predicate != null) q = q.Where(predicate);

            if (orderBy != null)
                q = orderBy(q);

            if (skip.HasValue) q = q.Skip(skip.Value);
            if (take.HasValue) q = q.Take(take.Value);

            return await q.AsNoTracking().ToListAsync(ct);
        }

        public virtual async Task<T?> GetByIdAsync(object[] keyValues, CancellationToken ct = default)
        {
            if (keyValues == null || keyValues.Length == 0)
                throw new ArgumentException("At least one key value must be provided", nameof(keyValues));

            // FindAsync returns the tracked entity (no AsNoTracking) so return as-is.
            var result = await _db.Set<T>().FindAsync(keyValues, ct);
            return result?.GetType() == typeof(T) ? result as T : result as T;
        }

        public virtual async Task<T?> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken ct = default,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> q = _db.Set<T>();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                    q = q.Include(include);
            }

            return await q.AsNoTracking().SingleOrDefaultAsync(predicate, ct);
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _db.Set<T>().Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            _db.Set<T>().UpdateRange(entities);
            await _db.SaveChangesAsync(ct);
        }
    }
}
