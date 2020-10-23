using Microsoft.EntityFrameworkCore;
using QueueDataAccess.Models;
using QueueingApi.Model.CounterTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QueueingApi.RepoAndServices.CounterTypes
{
    public class CounterTypeService
    {
        private readonly QueueDbContext _context;

        public CounterTypeService(QueueDbContext context)
        {
            _context = context;
        }

        public async Task CreateNewCounterType(UpdateCounterType value)
        {
            try
            {
                using (var trans = await _context.Database.BeginTransactionAsync())
                {
                    var newCounterType = new CounterType
                    {
                        CounterName = value.CounterTypeName,
                        CounterShortName = value.ShortName,
                        IsEndpoint = value.IsEndpoint              
                    };

                    await _context.CounterTypes.AddAsync(newCounterType);
                    await _context.SaveChangesAsync();

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task RemoveCounterType(int counterTypeId)
        {
            try
            {
                using (var trans = await _context.Database.BeginTransactionAsync())
                {

                    var counterType = await _context.CounterTypes
                        .FirstOrDefaultAsync(x => x.CounterTypeId == counterTypeId);

                    if(counterType == null)
                        throw new Exception("The counter type is not found!");

                    _context.CounterTypes.Remove(counterType);
                    await _context.SaveChangesAsync();

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task UpdateCounterType(int counterTypeId, UpdateCounterType value)
        {
            try
            {
                using (var trans = await _context.Database.BeginTransactionAsync())
                {
                    var counterType = await _context.CounterTypes
                        .FirstOrDefaultAsync(x => x.CounterTypeId == counterTypeId);

                    if(counterType == null)
                        throw new Exception("The counter type is not found!");

                    counterType.CounterShortName = value.ShortName;
                    counterType.CounterName = value.CounterTypeName;
                    counterType.IsEndpoint = value.IsEndpoint;

                    _context.CounterTypes.Update(counterType);
                    await _context.SaveChangesAsync();

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
