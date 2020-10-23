using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QueueDataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace QueueingApi.RepoAndServices.Devices
{
    public class DeviceApiService
    {
        private readonly QueueDbContext _context;

        public DeviceApiService(QueueDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Manually Saves the connection Id
        /// </summary>
        /// <param name="deviceSerial"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public async Task SaveConnectionId(string deviceSerial,
            string connectionId)
        {
            using (var transaction = 
                await _context.Database.BeginTransactionAsync())
            {
                var device = await _context.Devices
                    .FirstOrDefaultAsync(x => x.DeviceSerialNo.Equals(
                        deviceSerial, StringComparison.InvariantCultureIgnoreCase));

                if (device == null) return;

                device.ConnectionSerial = connectionId;

                _context.Devices.Update(device);
                await _context.SaveChangesAsync();

                transaction.Commit();
            }
        }

        /// <summary>
        /// Creates a new device depending on the type
        /// Does not create a new one if the serial is not unique
        /// </summary>
        /// <param name="deviceSerial"></param>
        /// <param name="isDesktop"></param>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        public async Task CreateDevice(string deviceSerial, 
            bool isDesktop, int counterNo)
        {
            using (var transaction = 
                await _context.Database.BeginTransactionAsync())
            {
                Counter counter = null;

                if (counterNo != 0)
                {
                    counter = await _context.Counters
                        .Include(x => x.CounterType)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.COunterNo == counterNo);

                    if (counter == null)
                        throw new NullReferenceException("The counter is not found!");
                }

                var device = await _context.Devices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DeviceSerialNo.Equals(deviceSerial, 
                        StringComparison.InvariantCultureIgnoreCase));

                // Update the device's counter type Id
                if (device != null)
                {
                    if (device.CouterTypeId != counter?.CounterType.CounterTypeId
                        || counterNo == 0)
                    {
                        device.CouterTypeId = counter?.CounterType.CounterTypeId;
                        _context.Devices.Update(device);
                        await _context.SaveChangesAsync();

                        transaction.Commit();

                        return;
                    }

                    return;
                }

                device = new Device
                {
                    DeviceSerialNo = deviceSerial,
                    IsDesktop = isDesktop,
                    CouterTypeId =  counter?.CounterType.CounterTypeId
                };

                await _context.Devices.AddAsync(device);
                await _context.SaveChangesAsync();

                transaction.Commit();
            }
        }
    }
}
