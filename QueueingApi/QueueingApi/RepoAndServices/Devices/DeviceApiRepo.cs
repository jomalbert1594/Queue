using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QueueDataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace QueueingApi.RepoAndServices.Devices
{
    public class DeviceApiRepo
    {
        private readonly QueueDbContext _context;

        public DeviceApiRepo(QueueDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the only desktop for Queueing System
        /// </summary>
        /// <returns></returns>
        public async Task<Device> GetDesktop()
        {
            return await _context.Devices
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IsDesktop);
        }

    }
}
