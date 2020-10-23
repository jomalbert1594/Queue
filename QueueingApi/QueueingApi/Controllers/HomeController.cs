using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QueueingApi.Helpers;
using QueueingApi.Model;

namespace QueueingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHubContext<QueueHub, IQueHub> _hubContext;
        private readonly CounterLocator _counter;

        public HomeController(IHubContext<QueueHub, IQueHub>  hubContext,
            CounterLocator counter)
        {
            _hubContext = hubContext;
            _counter = counter;
        }

        /// <summary>
        /// Greets all the clients
        /// </summary>
        /// <returns></returns>
        [HttpGet("Greet")]
        public async Task<IActionResult> Greet()
        {
            await _hubContext.Clients.All.ReceiveMessage("Hello!");
            return Ok();
        }
    }
}
