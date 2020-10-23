using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace QueueingApi.Helpers
{
    public interface IQueHub
    {
        Task ReceiveMessage(string message); // for testing only

        Task NextClient(int counterNo,
            string transName,
            int clientNo);

        Task TicketNo(string transaction,
            string shortTransName,
            int ticketNo);

        Task PrevClient(int counterNo,
            string transName,
            int clientNo);

        Task CurrentNo(int counterNo,
            string shortTransName,
            int clientNo);

        Task ReadNumberAgain(int counterNo,
            string shortTransName,
            int clientNo);

        Task InQueueClients(string seniorClients,
            string regularClients, int counterNo);

        Task PrevQueueClients(string seniorClients,
            string regularClients);
    }
}
