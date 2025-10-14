using System.Linq.Expressions;

namespace IconFilers.Api.IServices
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object[] keyValues, CancellationToken ct = default);

        Task<T?> SingleOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken ct = default,
            params Expression<Func<T, object>>[] includes);

        Task<IList<T>> FindAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            CancellationToken ct = default,
            params Expression<Func<T, object>>[] includes);

        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken ct = default);

        Task AddAsync(T entity, CancellationToken ct = default);

        Task UpdateAsync(T entity, CancellationToken ct = default);

        Task DeleteAsync(T entity, CancellationToken ct = default);

        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    }
}
