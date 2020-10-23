using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using QueueDataAccess.Models;

namespace QueueingApi.RepoAndServices.Transactions
{
    public class TransactionApiRepo
    {
        private readonly QueueDbContext _context;

        public TransactionApiRepo(QueueDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the list of devices' connection strings
        /// These are devices are related by connectionId
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        public async Task<List<string>> GetDevices(int counterNo)
        {
            // find the counter
            var counter = await _context.Counters
                .Include(x => x.CounterType)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.COunterNo == counterNo);

            if(counter == null)
                throw new NullReferenceException("The counter is not found!");

            var desktopDevice = await _context.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsDesktop);

            if(desktopDevice == null)
                throw new NullReferenceException("The desktop device is not found!");

            // find the devices and extract their connectionIds
            var mobileDev = await _context.Devices
                .AsNoTracking()
                .Where(x => x.CouterTypeId == counter.CounterType.CounterTypeId
                    && !string.IsNullOrEmpty(x.ConnectionSerial))
                .Select(x => x.ConnectionSerial)
                .ToListAsync();

            mobileDev.Add(desktopDevice.ConnectionSerial);

            return mobileDev;
        }

        /// <summary>
        /// Returns the previous clients
        /// 5 for senior, 5 for regular
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        public async Task<Tuple<string, string>> GetPreviousClients(int counterNo)
        {
            // Get the counter first
            var counter = await _context.Counters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.COunterNo == counterNo);

            if (counter == null)
                throw new NullReferenceException("The counter is not found!");

            // Get all the transactions that is not served yet for a
            // particular window, only take 10 
            // Get all of the senior priority numbers
            var seniorTrans = _context.Transactions
                .Include(x => x.TransPool)
                .ThenInclude(x => x.TransControl)
                .ThenInclude(x => x.CounterType)
                .AsNoTracking()
                .Where(x => x.TransPool.TransControl.CounterType.CounterTypeId == counter.CounterTypeId
                            && x.TransPool.TransactionDate.Date == DateTime.Now.Date
                            && x.TransPool.IsSpecial
                            && x.IsServed)
                .OrderByDescending(x => x.DateTimeOrdered)
                .Select(x => $"{x.Name}{x.PrioNo}");

            var seniorString = string.Join(" ", seniorTrans);

            // Get all of the regular priority numbers
            var regularTrans = _context.Transactions
                .Include(x => x.TransPool)
                .ThenInclude(x => x.TransControl)
                .ThenInclude(x => x.CounterType)
                .AsNoTracking()
                .Where(x => x.TransPool.TransControl.CounterType.CounterTypeId == counter.CounterTypeId
                            && x.TransPool.TransactionDate.Date == DateTime.Now.Date
                            && !x.TransPool.IsSpecial
                            && x.IsServed)
                .OrderByDescending(x => x.DateTimeOrdered)
                .Select(x => $"{x.Name}{x.PrioNo}");

            var regularString = string.Join(" ", regularTrans);

            return new Tuple<string, string>(seniorString, regularString);
        }

        /// <summary>
        /// Returns all the in queue clients
        /// 5 for senior, 5 for regular
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<string, string>> GetInQueueClients(int counterNo)
        {
            // get the counter
            var counter = await _context.Counters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.COunterNo == counterNo);

            if(counter == null)
                throw new NullReferenceException("The counter is not found!");

            // Get all the transactions that is not served yet for a
            // particular window, only take 10 
            // Get all of the senior priority numbers
            var seniorTrans = _context.Transactions
                .Include(x => x.TransPool)
                .ThenInclude(x => x.TransControl)
                .ThenInclude(x => x.CounterType)
                .AsNoTracking()
                .Where(x => x.TransPool.TransControl.CounterType.CounterTypeId == counter.CounterTypeId
                            && x.TransPool.TransactionDate.Date == DateTime.Now.Date
                            && x.TransPool.IsSpecial
                            && !x.IsServed)
                .OrderBy(x => x.DateTimeOrdered.Value.TimeOfDay)
                .Select(x => $"{x.Name}{x.PrioNo}");

            var seniorString = string.Join(" ", seniorTrans);

            // Get all of the regular priority numbers
            var regularTrans = _context.Transactions
                .Include(x => x.TransPool)
                .ThenInclude(x => x.TransControl)
                .ThenInclude(x => x.CounterType)
                .AsNoTracking()
                .Where(x => x.TransPool.TransControl.CounterType.CounterTypeId == counter.CounterTypeId
                            && x.TransPool.TransactionDate.Date == DateTime.Now.Date
                            && !x.TransPool.IsSpecial
                            && !x.IsServed)
                .OrderBy(x => x.DateTimeOrdered.Value.TimeOfDay)
                .Select(x => $"{x.Name}{x.PrioNo}");

            var regularString = string.Join(" ", regularTrans);

            return new Tuple<string, string>(seniorString, regularString);
        }

        /// <summary>
        /// Gets a particular transaction
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<Transaction> Get(int id)
        {
            return _context.Transactions.FirstOrDefaultAsync(x =>
                x.TransactionId == id && !x.IsDeleted);
        }

        /// <summary>
        /// Gets all undeleted transaction
        /// </summary>
        /// <returns></returns>
        public Task<List<Transaction>> GetAll()
        {
            return _context.Transactions.Where(x => !x.IsDeleted)
                .ToListAsync();
        }

        
    }
}
