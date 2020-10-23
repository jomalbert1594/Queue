using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QueueDataAccess.Models;

namespace QueueingApi.RepoAndServices.Transactions
{
    /// <summary>
    /// Database Transactions
    /// </summary>
    public class TransactionApiService
    {
        private readonly QueueDbContext _context;

        public TransactionApiService(QueueDbContext context)
        {
            _context = context;
        }
        
        #region Previous transaction

        /// <summary>
        /// Releases the Previous Number
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        public async Task<Tuple<string, int>> Prev(int counterNo)
        {
            using (var transaction =
                await _context.Database.BeginTransactionAsync())
            {
                // find the counter
                var counter = await _context.Counters
                    .FirstOrDefaultAsync(x => x.COunterNo == counterNo
                    && !x.IsDeleted);

                if(counter == null)
                    return new Tuple<string, int>("", 0);

                _context.Entry(counter).Property(x => x.RowVersion2).OriginalValue =
                    counter.RowVersion2;

                // find the transaction
                var trans = await _context.Transactions
                    .Include(x => x.PrevTrans)
                    .FirstOrDefaultAsync(x => x.TransactionId == counter.TransactionId);

                if(trans == null)
                    return new Tuple<string, int>("", 0);

                if(trans.PrevTrans == null)
                    return new Tuple<string, int>("", 0);

                counter.TransactionId = trans.PrevTrans.TransactionId;

                _context.Counters.Update(counter);

                await _context.SaveChangesAsync();
                transaction.Commit();

                return new Tuple<string, int>(trans.PrevTrans.Name,
                    trans.PrevTrans.PrioNo);
            }
        }

        #endregion

        #region Next Transaction

        /// <summary>
        /// Returns the next no. for the counter
        /// Different counter types different pool
        /// The operation should be alternative between special and regular
        /// of numbers
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        public async Task<Tuple<string, int>> NextNo(int counterNo)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var counter = await _context.Counters
                    .FirstOrDefaultAsync(x => x.COunterNo == counterNo
                        && !x.IsDeleted);

                if (counter == null)
                    return new Tuple<string, int>("", 0);

                _context.Entry(counter).Property(x => x.RowVersion2).OriginalValue = 
                    counter.RowVersion2;

                var curtrans = await _context.Transactions
                    .Include(x => x.NextTrans)
                    .FirstOrDefaultAsync(x => x.TransactionId == counter.TransactionId);

                // Find the Transaction controller
                var transControl = await _context.TransControls            
                    .FirstOrDefaultAsync(x => x.CounterTypeId == counter.CounterTypeId);

                if(transControl == null)
                    return new Tuple<string, int>("", 0);
           
                // 1. If there's an existing current transaction
                if (curtrans != null && transControl.IsSpecial != null)
                {
                    _context.Entry(curtrans).Property(x => x.RowVersion2).OriginalValue = 
                        curtrans.RowVersion2;

                    var newTrans = await NextWithExistingTransaction(counter,
                        curtrans, transControl);

                    if(newTrans == null)
                        return new Tuple<string, int>("" , 0);

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    return new Tuple<string, int>(
                        newTrans.Name, newTrans.PrioNo);
                }

                // counter , trans control 
                // 2. If this is an initial transaction 
                if (curtrans == null && transControl.IsSpecial != null)
                {

                    var newTrans = await NextWithNoTransaction(
                        counter, transControl);

                    if(newTrans == null)
                        return new Tuple<string, int>("", 0);

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    return new Tuple<string, int>(
                        newTrans.Name, newTrans.PrioNo);
                }
            }

            return new Tuple<string, int>("", 0);
        }

        private async Task<Transaction> NextWithExistingTransaction(Counter counter,
            Transaction curtrans, TransControl transControl)
        {
            // If there's already an existing next transaction
            if (curtrans.NextTrans != null)
            {
                counter.TransactionId = curtrans.NextId;

                _context.Counters.Update(counter);
                await _context.SaveChangesAsync();

                return curtrans.NextTrans;
            }

            // find the next pool, use the current pool if the next pool is not found
            var nextPool = await _context.TransPools
                .FirstOrDefaultAsync(x => x.TransControlId == transControl.TransControlId
                    && x.TransactionDate.Date == DateTime.Now.Date
                    && x.IsSpecial == !transControl.IsSpecial.Value) ??
                    await _context.TransPools
                .FirstOrDefaultAsync(x => x.TransControlId == transControl.TransControlId
                    && x.TransactionDate.Date == DateTime.Now.Date
                    && x.IsSpecial == transControl.IsSpecial.Value);


            var newTrans = await _context.Transactions
                .FirstOrDefaultAsync(x => x.TransPoolId == nextPool.TransPoolId
                                          && !x.IsServed
                                          && x.PrevId == null); // Find the transaction at the tail of the main chain

            // Use the current pool instead to get the new transaction
            if (newTrans == null)
            {
                var pool = await _context.TransPools
                    .FirstOrDefaultAsync(x => x.TransControlId == transControl.TransControlId
                                              && x.TransactionDate.Date == DateTime.Now.Date
                                              && x.IsSpecial == transControl.IsSpecial.Value);

                if (pool == null)
                    return null;

                newTrans = await _context.Transactions
                    .FirstOrDefaultAsync(x => x.TransPoolId == pool.TransPoolId
                                              && !x.IsServed
                                              && x.PrevId == null); // Find the transaction at the tail of the main chain
                nextPool = pool;
            }
            
            if (newTrans == null) // return null if there's really no transaction
                return null;

            var newNextTrans = await _context.Transactions
                .FirstOrDefaultAsync(x => x.TransactionId == newTrans.NextId);

            // The next transaction's next one will be the new tail of the main tail
            if (newNextTrans != null)
            {
                _context.Entry(newTrans).Property(x => x.RowVersion2).OriginalValue = 
                    newTrans.RowVersion2;
                _context.Entry(newNextTrans).Property(x => x.RowVersion2).OriginalValue = 
                    newNextTrans.RowVersion2;

                newNextTrans.PrevId = null;
                newTrans.NextId = null;

                _context.Transactions.Update(
                    newNextTrans);
            }

            newTrans.IsServed = true;

            // Link the new trans to the new chain belonging to a window
            curtrans.NextId = newTrans.TransactionId;
            newTrans.PrevId = curtrans.TransactionId;

            counter.TransactionId = newTrans.TransactionId;
            transControl.IsSpecial = nextPool.IsSpecial;

            _context.TransControls.Update(transControl);
            _context.Counters.Update(counter);
            _context.Transactions.Update(newTrans);
            _context.Transactions.Update(curtrans);

            return newTrans;
        }

        private async Task<Transaction> NextWithNoTransaction(Counter counter,
            TransControl transControl)
        {
            // Find the special transaction pool, if not found, get the counterpart
            var curTransPool = await _context.TransPools
                     .FirstOrDefaultAsync(x => x.TransControlId == transControl.TransControlId
                        && x.TransactionDate.Date == DateTime.Now.Date
                        && x.IsSpecial == !transControl.IsSpecial) ?? await _context.TransPools
                     .FirstOrDefaultAsync(x => x.TransControlId == transControl.TransControlId
                        && x.TransactionDate.Date == DateTime.Now.Date
                        && x.IsSpecial == transControl.IsSpecial);

            if (curTransPool == null)
                return null;

            // Remove the newTrans from the main chain
            var newTrans = await _context.Transactions
                .FirstOrDefaultAsync(x => x.TransPoolId == curTransPool.TransPoolId
                    && x.PrevId == null
                    && !x.IsServed);

            // Add the newTrans to the counter's chain
            // If the next transaction is not found, try getting a transaction from the next pool
            if (newTrans == null)
            {
                var pool = await _context.TransPools
                    .FirstOrDefaultAsync(x => x.TransControlId == transControl.TransControlId
                            && x.TransactionDate.Date == DateTime.Now.Date
                            && x.IsSpecial == !curTransPool.IsSpecial);

                if (pool == null)
                    return null;

                newTrans = await _context.Transactions
                    .FirstOrDefaultAsync(x => x.TransPoolId == pool.TransPoolId
                                              && !x.IsServed
                                              && x.PrevId == null);

                if (newTrans == null)
                    return null;

                curTransPool = pool;
            }

            var newNextTrans = await _context.Transactions
                .FirstOrDefaultAsync(x => x.TransactionId == newTrans.NextId);

            // Remove the new trans from the main chain
            if (newNextTrans != null)
            {
                _context.Entry(newTrans).Property(x => x.RowVersion2).OriginalValue = 
                    newTrans.RowVersion2;
                _context.Entry(newNextTrans).Property(x => x.RowVersion2).OriginalValue = 
                    newNextTrans.RowVersion2;

                newTrans.NextId = null;
                newNextTrans.PrevId = null;

                _context.Transactions.Update(newNextTrans);
            }

            newTrans.IsServed = true;

            counter.TransactionId = newTrans.TransactionId;
            transControl.IsSpecial = curTransPool.IsSpecial;

            _context.TransControls.Update(transControl);
            _context.Counters.Update(counter);
            _context.Transactions.Update(newTrans);

            return newTrans;
        }

        #endregion

        #region Printing

        /// <summary>
        /// Returns the next number
        /// Updates the transaction
        /// Creates a transaction if there's none
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<string, string, int>> PrintPrioNo(int counterTypeId, bool isSpecial)
        {
            using (var transaction =
                await _context.Database.BeginTransactionAsync())
            {
                var counterType = await _context.CounterTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CounterTypeId == counterTypeId);

                if(counterType == null)
                    return new Tuple<string, string, int>("", "", 0);

                // find out if there's an existing transpool
                var transControl = await _context.TransControls
                    .FirstOrDefaultAsync(x =>
                        x.CounterTypeId == counterType.CounterTypeId);

                // Create a transaction controller if there's not anything found
                if (transControl == null)
                {
                    var newTransControl = new TransControl
                    {
                        CounterTypeId = counterType.CounterTypeId,
                        IsSpecial = false
                    };

                    await _context.TransControls.AddAsync(newTransControl);
                    await _context.SaveChangesAsync();

                    transControl = newTransControl;
                }

                // find out if there's an existing trans pool
                var transPool = await _context.TransPools
                    .FirstOrDefaultAsync(x =>
                        x.TransControlId == transControl.TransControlId
                        && x.IsSpecial == isSpecial
                        && x.TransactionDate.Date == DateTime.Now.Date);

                // Create new transaction pool if there's nothing found
                if (transPool == null)
                {
                    var newTransPool = new TransPool
                    {
                        TransControlId = transControl.TransControlId,
                        IsSpecial = isSpecial,
                        TransactionDate = DateTime.Now
                    };

                    if (transControl.IsSpecial == null)
                        transControl.IsSpecial = false;

                    _context.TransControls.Update(transControl);

                    await _context.TransPools.AddAsync(newTransPool);
                    await _context.SaveChangesAsync(); // needed this to generate Id

                    transPool = newTransPool;
                }

                // Find out if there's any previous transaction that was printed
                var trans = await _context.Transactions
                    .FirstOrDefaultAsync(x => x.NextId == null
                        && x.TransPoolId == transPool.TransPoolId
                        && !x.IsServed);

                // Create new transaction and link it to the previous one if there's any
                if (trans != null)
                {
                    // Determine if the objects' version are in sync with what's in the database
                    _context.Entry(trans).Property(x => x.RowVersion2).OriginalValue = trans.RowVersion2;

                    // Save the last number generated
                    transPool.Last = transPool.Last + 1;

                    // increment the prio number before creating the new transaction
                    var newTransRes = await CreateNewTransaction(transPool.TransPoolId,
                        transPool.Last, isSpecial, counterType.CounterShortName);

                    var newTrans = newTransRes.Item2;

                    _context.Entry(newTrans).Property(x => x.RowVersion2).OriginalValue = newTrans.RowVersion2;

                    trans.NextId = newTrans.TransactionId;
                    newTrans.PrevId = trans.TransactionId;

                    _context.TransPools.Update(transPool);
                    _context.Transactions.Update(trans);
                    _context.Transactions.Update(newTrans);

                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    return new Tuple<string, string, int>(counterType.CounterName,
                        newTransRes.Item1, newTransRes.Item2.PrioNo);
                }

                // Update the trans pool to save the first and last number
                transPool.First = transPool.First + 1;
                transPool.Last = transPool.Last + 1;

                var newTransRes2 = await CreateNewTransaction(transPool.TransPoolId,
                    transPool.Last, isSpecial, counterType.CounterShortName);

                if(newTransRes2.Item2 == null || newTransRes2.Item2.TransactionId == 0)
                    return new Tuple<string, string, int>("", "", 0);

                _context.TransPools.Update(transPool);

                await _context.SaveChangesAsync();
                transaction.Commit(); // Commit all to the database

                return new Tuple<string, string, int>(counterType.CounterName,
                    newTransRes2.Item1,
                    newTransRes2.Item2.PrioNo);
            }
        }

        /// <summary>
        /// Create a new transaction based from its
        /// Transaction pool
        /// </summary>
        /// <param name="transPoolId"></param>
        /// <param name="prioNo"></param>
        /// <param name="isSpecial"></param>
        /// <param name="counterName"></param>
        /// <returns></returns>
        private async Task<Tuple<string, Transaction>> CreateNewTransaction(int transPoolId,
            int prioNo, bool isSpecial, string counterName)
        {
            var transactionName = counterName;

            // Add s to indicate that the transaction is special
            if (isSpecial)
            {
                var newTransName = new StringBuilder(transactionName);
                newTransName.Append("s");
                transactionName = newTransName.ToString();
            }

            var newTrans = new Transaction
            {
                PrioNo = prioNo,
                TransPoolId = transPoolId,
                Name = transactionName,
                DateTimeOrdered = DateTime.Now
            };

            await _context.Transactions.AddAsync(newTrans);
            await _context.SaveChangesAsync();

            return new Tuple<string, Transaction>(transactionName, newTrans);
        }

        #endregion

        #region Initialization
        public async Task<Tuple<int, string, int>> GetACurrentCounterNo(int counterNo)
        {
            using (var transaction = 
                await _context.Database.BeginTransactionAsync())
            {
                var counter = await _context.Counters
                    .FirstOrDefaultAsync(x => x.COunterNo == counterNo);

                if(counter == null)
                    return new Tuple<int, string, int>(counterNo, "", 0);

                // Check whether the object's version is in sync with the database's version
                _context.Entry(counter).Property(x => x.RowVersion2).OriginalValue = counter.RowVersion2;

                var trans = await _context.Transactions
                    .Include(x => x.TransPool)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => 
                        x.TransactionId == counter.TransactionId);

                var isPassed = !(trans == null || 
                    trans.TransPool?.TransactionDate.Date != DateTime.Now.Date);

                if (isPassed)
                    return new Tuple<int, string, int>(counterNo,
                        trans.Name, trans.PrioNo);

                counter.TransactionId = null;

                _context.Counters.Update(counter);

                await _context.SaveChangesAsync();

                transaction.Commit();

                return new Tuple<int, string, int>(counterNo, "", 0);
            }
        }

        /// <summary>
        /// Returns the numbers the counters are currently serving
        /// Returns CounterNo, with the transaction name and its number
        /// Resets if the numbers are not for the current day
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<int, Tuple<string, int>>> GetAllCurrentNo()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var counters = await _context.Counters
                    .ToListAsync();

                var counterDict = 
                    new Dictionary<int, Tuple<string, int>>();

                foreach (var counter in counters)
                {
                    // Check whether the object's version is in sync with the database's version
                    _context.Entry(counter).Property(x => x.RowVersion2).OriginalValue = counter.RowVersion2;

                    var trans = await _context.Transactions
                        .Include(x => x.TransPool)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.TransactionId == counter.TransactionId);

                    // reset the counter if the transaction is not for today
                    if (trans == null || 
                          trans.TransPool?.TransactionDate.Date != DateTime.Now.Date)
                    {
                        counter.TransactionId = null;

                        _context.Counters.Update(counter);

                        counterDict.Add(counter.COunterNo,
                            new Tuple<string, int>("", 0));
                    }
                    else
                    {
                        counterDict.Add(counter.COunterNo,
                            new Tuple<string, int>(trans.Name, trans.PrioNo));
                    }
                                  
                }

                await _context.SaveChangesAsync();
                transaction.Commit();

                return counterDict;
            }
        }

        #endregion

        #region Passing transaction to the Cashier or payments window

        /// <summary>
        /// Passes a transaction to the cashier
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        public async Task<Tuple<string, int>> PassTransactionToCashier(int counterNo)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var counter = await _context.Counters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.COunterNo == counterNo);

                if (counter == null)
                    return new Tuple<string, int>("", 0);

                // find the transaction to be passed
                var trans = await _context.Transactions
                    .Include(x => x.TransPool)
                    .FirstOrDefaultAsync(x => x.TransactionId == counter.TransactionId);

                // Make sure that the transaction is good for the current day
                if (trans == null)
                    return new Tuple<string, int>("", 0);

                var prevTrans = await _context.Transactions
                        .FirstOrDefaultAsync(x => x.TransactionId == trans.PrevId);

                var nextTrans = await _context.Transactions
                    .FirstOrDefaultAsync(x => x.TransactionId == trans.NextId);

                // Determine if the version is in sync with the database
                _context.Entry(trans).Property(x => x.RowVersion2).OriginalValue =
                trans.RowVersion2;

                // Find if there's an existing Trans control for payments
                var cashierTransControl = await _context.TransControls
                    .Include(x => x.CounterType)
                    .FirstOrDefaultAsync(x => x.CounterType.IsEndpoint);

                // Create a transaction control if not found
                if (cashierTransControl == null)
                {
                    var cashierCounterType = await _context.CounterTypes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.IsEndpoint);

                    var newCashierTransControl = new TransControl
                    {
                        CounterTypeId = cashierCounterType.CounterTypeId,
                        IsSpecial = false
                    };

                    await _context.TransControls.AddAsync(newCashierTransControl);
                    await _context.SaveChangesAsync();

                    cashierTransControl = newCashierTransControl;
                }

                // find if there's an existing trans pool for payments
                var cashierTransPool = await _context.TransPools
                    .FirstOrDefaultAsync(x => x.TransControlId == cashierTransControl.TransControlId
                        && x.TransactionDate.Date == DateTime.Now.Date
                        && x.IsSpecial == trans.TransPool.IsSpecial);

                // Create a trans pool if not found
                if (cashierTransPool == null)
                {
                    var newCashierTransPool = new TransPool
                    {
                        TransControlId = cashierTransControl.TransControlId,
                        IsSpecial = trans.TransPool.IsSpecial,
                        First = 1,
                        Last = 0,
                        TransactionDate = DateTime.Now.Date
                    };

                    await _context.TransPools.AddAsync(newCashierTransPool);
                    await _context.SaveChangesAsync();

                    cashierTransPool = newCashierTransPool;
                }

                // Find the tail of the main chain of payments
                var cashierTailTrans = await _context.Transactions
                    .FirstOrDefaultAsync(x => !x.IsServed
                        && x.TransPool.TransPoolId == cashierTransPool.TransPoolId
                        && x.NextId == null);

                if (cashierTailTrans != null)
                {
                    // Determine if the version of object is in sync with what with the database
                    _context.Entry(cashierTailTrans).Property(x => x.RowVersion2).OriginalValue =
                        cashierTailTrans.RowVersion2;

                    // establish the link between the cashier's tail and the passed transaction
                    cashierTailTrans.NextId = trans.TransactionId;
                    trans.PrevId = cashierTailTrans.TransactionId;
                    trans.NextId = null; // Since this serves as the new tail of the cashier pool chain

                    _context.Transactions.Update(cashierTailTrans);
                }
                else
                {
                    // Happens when the transaction is the first one in the cashier
                    trans.NextId = null;
                    trans.PrevId = null;
                }

                // Sever ties between the neighboring transactions and the passed transaction
                var transName = "";
                var prioNo = 0;

                if (prevTrans == null && nextTrans == null)
                {
                    counter.TransactionId = null;
                }

                if (prevTrans != null && nextTrans != null)
                {
                    _context.Entry(prevTrans).Property(x => x.RowVersion2).OriginalValue =
                        prevTrans.RowVersion2;
                    _context.Entry(nextTrans).Property(x => x.RowVersion2).OriginalValue =
                        nextTrans.RowVersion2;

                    nextTrans.PrevId = prevTrans.TransactionId;
                    prevTrans.NextId = nextTrans.TransactionId;
                                
                    // Pass the next transaction to the counter
                    counter.TransactionId = nextTrans.TransactionId;
                    transName = nextTrans.Name;
                    prioNo = nextTrans.PrioNo;

                    // Update the prev trans and next trans
                    _context.Transactions.Update(prevTrans);
                    _context.Transactions.Update(nextTrans);
                }

                // If there is a previous transaction and no next trans
                if (prevTrans != null && nextTrans == null)
                {
                    _context.Entry(prevTrans).Property(x => x.RowVersion2).OriginalValue =
                        prevTrans.RowVersion2;

                    prevTrans.NextId = null;

                    // Pass the previous transaction to the counter
                    counter.TransactionId = prevTrans.TransactionId;

                    _context.Transactions.Update(prevTrans);
                }

                // If there is no prev transaction but there is a next transaction
                if (prevTrans == null && nextTrans != null)
                {
                    _context.Entry(nextTrans).Property(x => x.RowVersion2).OriginalValue =
                        nextTrans.RowVersion2;

                    nextTrans.PrevId = null;

                    // Pass the next transaction to the counter
                    counter.TransactionId = nextTrans.TransactionId;
                    transName = nextTrans.Name;
                    prioNo = nextTrans.PrioNo;

                    _context.Transactions.Update(nextTrans);
                }

                // Transfer the pool to the cashier's pool
                trans.TransPoolId = cashierTransPool.TransPoolId;
                trans.IsServed = false;
                trans.DateTimeOrdered = DateTime.Now; // The time date and time it is transfered

                _context.Counters.Update(counter);
                _context.Transactions.Update(trans);

                await _context.SaveChangesAsync();

                transaction.Commit();

                return new Tuple<string, int>(transName, prioNo);
            }
        }

        #endregion
    }
}