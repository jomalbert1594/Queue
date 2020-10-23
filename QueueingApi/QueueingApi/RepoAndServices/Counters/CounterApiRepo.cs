using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QueueDataAccess.Models;
using QueueingApi.Model.Counters;

namespace QueueingApi.RepoAndServices.Counters
{
    public class CounterApiRepo
    {
        private readonly QueueDbContext _context;

        public CounterApiRepo(QueueDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a particular counter
        /// </summary>
        /// <param name="counterTypeNo"></param>
        /// <returns></returns>
        public Task<CounterInfo> Get(int counterId)
        {
            return _context.Counters
                .Include(x => x.CounterType)
                .AsNoTracking()
                .Where(x => !x.IsDeleted &&
                    x.CounterId == counterId)
                .Select(x => new CounterInfo{ 
                    CounterId = x.CounterId,
                    CounterName = x.CounterName,
                    CounterNo = x.COunterNo,
                    CounterType = x.CounterType.CounterName
                }).FirstOrDefaultAsync();
        }

        public Task<UpdateCounter> UpdateInfo(int counterId)
        {
            return _context.Counters
                .Include(x => x.CounterType)
                .AsNoTracking()
                .Where(x => 
                    x.CounterId == counterId)
                .Select(x => new UpdateCounter{ 
                    CounterName = x.CounterName,
                    CounterNo = x.COunterNo,
                    CounterTypeId = x.CounterTypeId ?? 0
                }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all undeleted counter
        /// </summary>
        /// <returns></returns>
        public Task<List<CounterInfo>> GetAll()
        {
            return _context.Counters
                .Include(x => x.CounterType)
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new CounterInfo{
                    CounterId = x.CounterId,
                    CounterName = x.CounterName,
                    CounterNo = x.COunterNo,
                    CounterType = x.CounterType.CounterName
                })
                .ToListAsync();
        }
    }
}
