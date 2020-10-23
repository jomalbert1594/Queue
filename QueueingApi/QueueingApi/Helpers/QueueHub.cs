using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QueueDataAccess.Models;

namespace QueueingApi.Helpers
{
    public class QueueHub:Hub<IQueHub>
    {

        private readonly QueueDbContext _context;
        public QueueHub(QueueDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Event when a client connects to this server
        /// Updates the existing device
        /// Does not register the unrecognized device
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var connectionId = Context.ConnectionId;
                var deviceSerial = Context.User.Identity.Name;

                var device = await _context.Devices
                    .FirstOrDefaultAsync(x => x.DeviceSerialNo.Equals(deviceSerial, 
                        StringComparison.InvariantCultureIgnoreCase));

                if (device == null) return;

                device.ConnectionSerial = connectionId;

                _context.Devices.Update(device);
                await _context.SaveChangesAsync();

                transaction.Commit();

                await base.OnConnectedAsync();
            }
        }

        /// <summary>
        /// Sends the connection Id to the client
        /// </summary>
        /// <returns></returns>
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        /// <summary>
        /// Sends the next client
        /// To a particular counter
        /// </summary>
        /// <param name="counterNo"></param>
        /// <param name="transName"></param>
        /// <param name="clientNo"></param>
        /// <returns></returns>
        public async Task Next(int counterNo, 
            string transName,
            int clientNo)
        {
            
            await Clients.All.NextClient(counterNo, 
                transName, clientNo);
        }

        /// <summary>
        /// Sends the prev client
        /// To a particular counter
        /// </summary>
        /// <param name="counterNo"></param>
        /// <param name="transName"></param>
        /// <param name="clientNo"></param>
        /// <returns></returns>
        public async Task PrevClient(int counterNo,
            string transName,
            int clientNo)
        {
            await Clients.All.PrevClient(counterNo, 
                transName, clientNo);
        }

        /// <summary>
        /// Sends the ticket no. to be printed
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="shorTransName"></param>
        /// <param name="ticketNo"></param>
        /// <returns></returns>
        public async Task TicketNo(string transaction,
            string shorTransName,
            int ticketNo)
        {
            await Clients.All.TicketNo(transaction,
                shorTransName, ticketNo);
        }

        /// <summary>
        /// Sends the current number of the counter
        /// </summary>
        /// <param name="counterNo"></param>
        /// <param name="shortTransName"></param>
        /// <param name="clientNo"></param>
        /// <returns></returns>
        public async Task CurrentNo(int counterNo, 
            string  shortTransName,int clientNo)
        {
            await Clients.All.CurrentNo(counterNo, 
                shortTransName, clientNo);
        }

        /// <summary>
        /// Returns the in queue clients
        /// </summary>
        /// <param name="seniorClients"></param>
        /// <param name="regularClients"></param>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        public async Task InQueueClients(string seniorClients,
            string regularClients, int counterNo)
        {
            await Clients.All.InQueueClients(seniorClients,
                regularClients, counterNo);
        }

        /// <summary>s
        /// Returns the previous clients
        /// </summary>
        /// <param name="seniorClients"></param>
        /// <param name="regularClients"></param>
        /// <returns></returns>
        public async Task PrevQueueClients(string seniorClients,
            string regularClients)
        {
            await Clients.All.PrevQueueClients(seniorClients,
                regularClients);
        }
    }
}
