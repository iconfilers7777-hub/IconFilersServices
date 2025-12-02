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
        public async Task<ActionResult<IEnumerable<string>>> GetStatuses()
        {
            try
            {
                const string cacheKey = "StatusesCache";

                if (_cache.TryGetValue(cacheKey, out IEnumerable<string> cachedStatusNames))
                {
                    return new ActionResult<IEnumerable<string>>(cachedStatusNames);
                }

                var statuses = await _context.Statuses
                       .FromSqlRaw("EXEC GetClientsStatus")
                       .AsNoTracking()
                       .ToListAsync();

                var statusNames = statuses.Select(s => s.StatusName).ToList();


                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, statusNames, cacheOptions);

                return new ActionResult<IEnumerable<string>>(statusNames);
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
        public async Task<IEnumerable<string>> GetRoles()
        {
            try
            {
                const string cacheKey = "RolesCache";

                if (_cache.TryGetValue(cacheKey, out IEnumerable<string> cachedRoles))
                {
                    return cachedRoles;
                }

                var rolesWithId = await _context.Roles
                    .AsNoTracking()
                    .ToListAsync();

                var roles = rolesWithId.Select(r => r.Name).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _cache.Set(cacheKey, roles, cacheOptions);

                return roles;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching roles.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while fetching roles.", dbEx);
            }
            catch (DBConcurrencyException dbEx)
            {
                throw new Exception("A concurrency error occurred while fetching roles.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching roles.", ex);
            }
        }
        public async Task<IEnumerable<DocTypes>> GetTypes()
        {
            try
            {
                var Types = await _context.Types
                       .FromSqlRaw("EXEC GetDocumentTypes")
                       .AsNoTracking()
                       .ToListAsync();

                return Types;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching Types.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while fetching Types.", dbEx);
            }
            catch (DBConcurrencyException dbEx)
            {
                throw new Exception("A concurrency error occurred while fetching Types.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching Types.", ex);
            }
        }
        public async Task<IEnumerable<DocCount>> GetDocumentsCount()
        {
            try
            {
                var Count = await _context.DocCount
                       .FromSqlRaw("EXEC GetDocumentsCount")
                       .AsNoTracking()
                       .ToListAsync();

                return Count;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching Count.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while fetching Count.", dbEx);
            }
            catch (DBConcurrencyException dbEx)
            {
                throw new Exception("A concurrency error occurred while fetching Count.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching Count.", ex);
            }
        }
        public async Task<IEnumerable<DocCount>> GetVerifiedDocumentsCount()
        {
            try
            {
                var Count = await _context.DocCount
                       .FromSqlRaw("EXEC GetVerifiedDocumentsCount")
                       .AsNoTracking()
                       .ToListAsync();

                return Count;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching Count.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while fetching Count.", dbEx);
            }
            catch (DBConcurrencyException dbEx)
            {
                throw new Exception("A concurrency error occurred while fetching Count.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching Count.", ex);
            }
        }
        public async Task<IEnumerable<DocCount>> GetPendingDocumentsCount()
        {
            try
            {
                var Count = await _context.DocCount
                       .FromSqlRaw("EXEC GetPendingDocumentsCount")
                       .AsNoTracking()
                       .ToListAsync();

                return Count;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching Count.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while fetching Count.", dbEx);
            }
            catch (DBConcurrencyException dbEx)
            {
                throw new Exception("A concurrency error occurred while fetching Count.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching Count.", ex);
            }
        }
        public async Task<IEnumerable<DocCount>> GetRejectedDocumentsCount()
        {
            try
            {
                var Count = await _context.DocCount
                       .FromSqlRaw("EXEC GetRejectedDocumentsCount")
                       .AsNoTracking()
                       .ToListAsync();

                return Count;
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Database error occurred while fetching Count.", sqlEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("A database update error occurred while fetching Count.", dbEx);
            }
            catch (DBConcurrencyException dbEx)
            {
                throw new Exception("A concurrency error occurred while fetching Count.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching Count.", ex);
            }
        }
    }
}
