using Microsoft.EntityFrameworkCore;
using QueueDataAccess.Models;
using QueueingApi.Model.CounterTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueueingApi.RepoAndServices.CounterTypes
{
    public class CounterTypeRepo
    {

        private readonly QueueDbContext _context;

        public CounterTypeRepo(QueueDbContext context)
        {
            _context = context;
        }


        public Task<CounterTypeInfo> GetCounterTypeInfo(int id)
        {
            try
            {
                return _context.CounterTypes
                    .AsNoTracking()
                    .Where(x => x.CounterTypeId == id)
                    .Select(x => new CounterTypeInfo{ 
                        CounterTypeId = x.CounterTypeId,
                        CounterTypeName = x.CounterName,
                        IsEndpoint = x.IsEndpoint,
                        ShortName = x.CounterShortName
                    }).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        } 


        public Task<UpdateCounterType> GetCounterTypeUpdateInfo(int id)
        {
            try
            {
                return _context.CounterTypes
                    .AsNoTracking()
                    .Where(x => x.CounterTypeId == id)
                    .Select(x => new UpdateCounterType{ 
                        CounterTypeName = x.CounterName,
                        IsEndpoint = x.IsEndpoint,
                        ShortName = x.CounterShortName
                    }).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Task<List<CounterTypeInfo>> GetCounterTypes()
        {
            try
            {
                return _context.CounterTypes
                    .AsNoTracking()
                    .Select(x => new CounterTypeInfo{ 
                        CounterTypeId = x.CounterTypeId,
                        CounterTypeName = x.CounterName,
                        IsEndpoint = x.IsEndpoint,
                        ShortName = x.CounterShortName
                    }).ToListAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
