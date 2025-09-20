using IconFilers.Api.IServices;
using IconFilers.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace IconFilers.Api.Services
{
    public class WorkFlowService : IWorkflow
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        public WorkFlowService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }
        public async Task<ActionResult<IEnumerable<Status>>> GetStatuses()
        {
            try
            {
                const string cacheKey = "StatusesCache";

                if (_cache.TryGetValue(cacheKey, out IEnumerable<Status> cachedStatuses))
                {
                    return new ActionResult<IEnumerable<Status>>(cachedStatuses);
                }


                var statuses = await _context.Statuses
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Category)
                    .ThenBy(s => s.StatusName)
                    .ToListAsync();

                statuses ??= new List<Status>();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, statuses, cacheOptions);

                return new ActionResult<IEnumerable<Status>>(statuses);
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching statuses.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while fetching statuses.", dbEx);
            }
            catch (DBConcurrencyException dbex)
            {
                throw new Exception("A concurrency error occurred while fetching statuses.", dbex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching statuses.", ex);
            }
        }
    }
}
