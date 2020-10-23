using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QueueDataAccess.Models;
using QueueingApi.Model.Counters;

namespace QueueingApi.RepoAndServices.Counters
{
    public class CounterApiService
    {
        private readonly QueueDbContext _context;
        public CounterApiService(QueueDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates new counter for the queue
        /// </summary>
        /// <param name="newCounter"></param>
        /// <returns></returns>
        public async Task Create(UpdateCounter value)
        {

            try
            {
                // rollbacks if there are errors
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    var newCounter = new Counter
                    {
                        CounterName = value.CounterName,
                        COunterNo = value.CounterNo,
                        CounterTypeId = value.CounterTypeId
                    };

                    await _context.Counters.AddAsync(newCounter);
                    await _context.SaveChangesAsync();

                    transaction.Commit(); // Commits to the database
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Updates the selected counter
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public async Task Update(UpdateCounter counter, int counterId)
        {
            try
            {
                // rollbacks if there are errors
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    var updtCounter = await _context.Counters.FirstOrDefaultAsync(
                        x => x.CounterId == counterId);

                    if(updtCounter == null)
                        throw new Exception("The counter is not found!");

                    // Update the properties
                    updtCounter.COunterNo = counter.CounterNo;
                    updtCounter.CounterName = counter.CounterName;
                    updtCounter.CounterTypeId = counter.CounterTypeId;

                    _context.Counters.Update(updtCounter);
                    await _context.SaveChangesAsync();

                    transaction.Commit(); // Commits to the database
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Deletes a counter
        /// Soft Delete
        /// </summary>
        /// <param name="counter"></param>
        /// <returns></returns>
        public async Task Delete(int counterId)
        {
            // rollbacks if there are errors
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var updtCounter = await _context.Counters.FirstOrDefaultAsync(
                    x => x.CounterId == counterId);

                if(updtCounter == null)
                    throw new Exception("The counter is not found!");
          
                updtCounter.IsDeleted = true; // just soft delete

                _context.Counters.Update(updtCounter);
                await _context.SaveChangesAsync();

                transaction.Commit(); // Commits to the database
            }
        }
    }
}
