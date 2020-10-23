using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QueueingApi.Helpers;
using QueueingApi.RepoAndServices.Transactions;
using QueueingApi.RepoAndServices.Devices;
using Microsoft.AspNetCore.Authorization;
using QueueingApi.RepoAndServices.Counters;

namespace QueueingApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class QueueController : ControllerBase
    {
        private readonly TransactionApiService _transService;
        private readonly TransactionApiRepo _transactionRepo;
        private readonly DeviceApiService _devService;
        private readonly DeviceApiRepo _devRepo;
        private readonly CounterApiRepo _counterRepo;
        private readonly IHubContext<QueueHub, IQueHub> _hubContext;

        public QueueController(TransactionApiService transService,
            TransactionApiRepo transactionRepo,
            DeviceApiService devService,
            DeviceApiRepo devRepo,
            CounterApiRepo counterRepo,
            IHubContext<QueueHub, IQueHub> hubContext)
        {
            _transService = transService;
            _transactionRepo = transactionRepo;
            _devService = devService;
            _devRepo = devRepo;
            _counterRepo = counterRepo;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Returns the Access token to allow the access
        /// of the device to the API
        /// Create a Device data for tracking purposes during real-time comm
        /// </summary>
        /// <param name="deviceSerial"></param>
        /// <param name="isDesktop"></param>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        [HttpGet("DeviceLog")]
        public async Task<IActionResult> AuthorizeDevice(string deviceSerial, 
            bool isDesktop, int counterNo)
        {
            //const string tokenIssuerUri =
            //    "http://localhost:62710/";

            //const string tokenIssuerUri =
            //    "http://192.168.2.6/queue/";

            const string tokenIssuerUri =
                "http://192.168.1.110/queue/";

            var tokenIssuer = new TokenIssuer();

            var token = tokenIssuer.GenerateToken(
                deviceSerial, tokenIssuerUri);

            // Create a new Device if the serial is unique
            await _devService.CreateDevice(
                deviceSerial, isDesktop,counterNo);

            return Ok(token);
        }

        /// <summary>
        /// Manually saves the connection Id for a device
        /// </summary>
        /// <param name="deviceSerial"></param>
        /// <param name="connectionId"></param>
        /// <returns></return
        [HttpGet("ConnectionId", Name = nameof(SaveConnectionId))]
        public async Task<IActionResult> SaveConnectionId(string deviceSerial, 
            string connectionId)
        {
            await _devService.SaveConnectionId(deviceSerial, connectionId);
            return Ok();
        }

        /// <summary>
        /// Returns the current number of a counter
        /// This will be used by the mobile app
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        [HttpGet("Counter/CurrentNo")]
        public async Task<IActionResult> GetCurrCounterNo(int counterNo)
        {
            var connectionId = HttpContext.Request.Headers["Connection-Id"];

            var counterandCurr = await _transService.GetACurrentCounterNo(counterNo);

            // look for the desktop device
            var device = await _devRepo.GetDesktop();

            if (device == null)
                throw new NullReferenceException("The desktop device is not found");

            // Broadcast the waiting clients and previous clients
            var waitingClients = await BroadcastWaitingClients(counterNo);

            var prevClients = await BroadcastPrevClients(counterNo);

            if (counterandCurr.Item3 == 0
                || string.IsNullOrWhiteSpace(counterandCurr.Item2))
                return Ok($"{waitingClients}\n{prevClients}");

            // broadcast via signalR
            if (!string.IsNullOrEmpty(connectionId))
                await _hubContext.Clients.Client(connectionId).CurrentNo(
                    counterandCurr.Item1, counterandCurr.Item2, counterandCurr.Item3);

            return Ok($"{waitingClients}\n{prevClients}");
        }

        /// <summary>
        /// Returns the waiting clients and broadcasts them
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        private async Task<string> BroadcastWaitingClients(int counterNo)
        {
            // Get all the waiting number for a particular window
            var (seniorClients, regularClients) = await
                _transactionRepo.GetInQueueClients(counterNo);

            // Get all the connectionId related to the counter
            //broadcast the waiting clients

            var connectionIds = await _transactionRepo.GetDevices(counterNo);

            if(connectionIds?.Count > 0)
                await _hubContext.Clients.Clients(connectionIds).InQueueClients(
                    seniorClients, regularClients, counterNo);

            var clients = $"{seniorClients}\n{regularClients}";

            return clients;
        }

        /// <summary>
        /// Returns the previous clients
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        private async Task<string> BroadcastPrevClients(int counterNo)
        {
            // Get all the previous client numbers
            var (prevSeniorClients, prevRegClients) = await
                _transactionRepo.GetPreviousClients(counterNo);

            var connectionIds = await _transactionRepo.GetDevices(counterNo);

            // broadcast them to the related devices
            if (connectionIds?.Count > 0)
                await _hubContext.Clients.Clients(connectionIds).PrevQueueClients(
                    prevSeniorClients, prevRegClients);

            var prevClients = $"{prevSeniorClients}\n{prevRegClients}";

            return prevClients;
        }

        /// <summary>
        /// Returns the current number of a counter
        /// </summary>
        /// <returns></returns>
        [HttpGet("Counter/Numbers")]
        public async Task<IActionResult> CoutersCurrNo()
        {
            var connectionId = HttpContext.Request.Headers["Connection-Id"];

            if (string.IsNullOrEmpty(connectionId))
                return Ok();

            var currentNos = await _transService.GetAllCurrentNo();

            // Broadcast the number to a counter
            foreach (var currentNo in currentNos)
            {
                // do not broadcast if the string key is empty
                // this contains the name of the transaction (abbreviated)
                if (string.IsNullOrWhiteSpace(currentNo.Value.Item1))
                    continue;
              
                if(!string.IsNullOrEmpty(connectionId))
                    await _hubContext.Clients.Client(connectionId).CurrentNo(
                        currentNo.Key,
                        currentNo.Value.Item1,
                        currentNo.Value.Item2);
            }

            // broadcasts the waiting clients or the payments
            var waitingClients = await BroadcastWaitingClients(7); 

            return Ok(waitingClients);
        }

        /// <summary>
        /// Returns the latest
        /// client No., last in the queue
        /// broadcasts the ticket no
        /// </summary>
        /// <returns></returns>
        [HttpGet("Ticket", Name=nameof(PrintPrioNo))]
        public async Task<IActionResult> PrintPrioNo(int counterTypeId, bool isSpecial)
        {
            var connectionId = HttpContext.Request.Headers["Connection-Id"];

            var ticketNo = await _transService.PrintPrioNo(counterTypeId, isSpecial);

            var counter = await _counterRepo.Get(counterTypeId);

            if(counter == null)
                throw new NullReferenceException("The counter is not found");

            // look for the desktop device
            var device = await _devRepo.GetDesktop();

            if (device == null)
                throw new NullReferenceException("The desktop device is not found");

            if (string.IsNullOrWhiteSpace(ticketNo.Item1)
                || string.IsNullOrWhiteSpace(ticketNo.Item2)
                || ticketNo.Item3 == 0)
                return Ok();

            // broadcasts the ticketNo to the ticketing app
            if (!string.IsNullOrEmpty(connectionId))
                await _hubContext.Clients.Client(connectionId).TicketNo(ticketNo.Item1,
                    ticketNo.Item2, ticketNo.Item3);

            await BroadcastWaitingClients(counter.CounterNo);

            return Ok();
        }

        /// <summary>
        /// Returns the next available client no
        /// broadcasts the next one (counterNo, transactionName, clientNo)
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        [HttpGet("Next", Name = nameof(Next))]
        public async Task<IActionResult> Next(int counterNo)
        {
            var connectionId = HttpContext.Request.Headers["Connection-Id"];

            // look for the desktop device
            var device = await _devRepo.GetDesktop();

            if (device == null)
                throw new NullReferenceException("The desktop device is not found");

            var nextNo = await _transService.NextNo(counterNo);

            // Broadcast waiting clients
            var waitingClients = await BroadcastWaitingClients(counterNo);

            var prevClients = await BroadcastPrevClients(counterNo);

            if (nextNo.Item2 == 0 ||
                string.IsNullOrWhiteSpace(nextNo.Item1))
                return Ok($"{waitingClients}\n{prevClients}");

            // Broadcast the number
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Clients(
                    connectionId, device.ConnectionSerial).NextClient(counterNo,
                    nextNo.Item1, nextNo.Item2);
            }
            else
            {
                await _hubContext.Clients.Client(device.ConnectionSerial).NextClient(counterNo,
                    nextNo.Item1, nextNo.Item2);
            }

            return Ok($"{waitingClients}\n{prevClients}");
        }

        /// <summary>
        /// Returns the prev client no
        /// broadcasts the prev one
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        [HttpGet("Prev", Name = nameof(Prev))]
        public async Task<IActionResult> Prev(int counterNo)
        {
            var connectionId = HttpContext.Request.Headers["Connection-Id"];

            // look for the desktop device
            var device = await _devRepo.GetDesktop();

            if (device == null)
                throw new NullReferenceException("The desktop device is not found");

            var prevNo = await _transService.Prev(counterNo);

            // Broadcast waiting clients
            var waitingClients = await BroadcastWaitingClients(counterNo);

            var prevClients = await BroadcastPrevClients(counterNo);

            if (prevNo.Item2 == 0 ||
                string.IsNullOrWhiteSpace(prevNo.Item1))
                return Ok($"{waitingClients}\n{prevClients}");

            // Broadcast the number
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Clients(
                    connectionId, device.ConnectionSerial).PrevClient(counterNo,
                    prevNo.Item1, prevNo.Item2);
            }
            else
            {
                await _hubContext.Clients.Clients(device.ConnectionSerial).PrevClient(counterNo,
                    prevNo.Item1, prevNo.Item2);
            }

            return Ok($"{waitingClients}\n{prevClients}");
        }

        /// <summary>
        /// Passes the transaction being handled by a counter
        /// to the cashier's pool
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        [HttpGet("Cashier", Name = nameof(PassTransToCashier))]
        public async Task<IActionResult> PassTransToCashier(int counterNo)
        {
            var connectionId = HttpContext.Request.Headers["Connection-Id"];

            // look for the desktop device
            var device = await _devRepo.GetDesktop();

            if (device == null)
                throw new NullReferenceException("The desktop device is not found");

            var result = await _transService
                .PassTransactionToCashier(counterNo);

            // Broadcast waiting and served clients to the cashier
            var waitingClients = await BroadcastWaitingClients(7);

            var prevClients = await BroadcastPrevClients(counterNo);

            // Broadcast the waiting clients to the devices
            await BroadcastWaitingClients(counterNo);

            // Reset the counter no to zero
            if (string.IsNullOrWhiteSpace(result.Item1))
                return Ok($"{waitingClients}\n{prevClients}");

            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Clients(
                    connectionId, device.ConnectionSerial).NextClient(counterNo,
                    result.Item1, result.Item2);
            }
            else
            {
                await _hubContext.Clients.Client(device.ConnectionSerial).NextClient(counterNo,
                    result.Item1, result.Item2);
            }

            return Ok($"{waitingClients}\n{prevClients}");
        }

        /// <summary>
        /// Broadcasts the number again to the caller of this endpoint
        /// </summary>
        /// <param name="counterNo"></param>
        /// <returns></returns>
        [HttpGet("Counter/NumberSpeech", Name = nameof(ReadNumberAgain))]
        public async Task<IActionResult> ReadNumberAgain(int counterNo)
        {
            var connectionId = HttpContext.Request.Headers["Connection-Id"];

            // look for the desktop device
            var device = await _devRepo.GetDesktop();

            if (device == null) return Ok();

            var counterandCurr = await _transService.GetACurrentCounterNo(counterNo);

            if (counterandCurr.Item3 == 0
                || string.IsNullOrWhiteSpace(counterandCurr.Item2))
                return Ok();

            // broadcast via signalR
            await _hubContext.Clients.Clients(connectionId, device.ConnectionSerial).ReadNumberAgain(
                counterandCurr.Item1, counterandCurr.Item2, counterandCurr.Item3);

            return Ok();
        }
    }
}
