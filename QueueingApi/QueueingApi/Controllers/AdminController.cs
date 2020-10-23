using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QueueingApi.Model.Counters;
using QueueingApi.Model.CounterTypes;
using QueueingApi.RepoAndServices.Counters;
using QueueingApi.RepoAndServices.CounterTypes;

namespace QueueingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly CounterApiRepo _cRepo;
        private readonly CounterApiService _cService;
        private readonly CounterTypeRepo _ctRepo;
        private readonly CounterTypeService _ctService;

        public AdminController(CounterApiRepo c, CounterApiService cs,
            CounterTypeRepo ct, CounterTypeService cts)
        {
            _cRepo = c;
            _cService = cs;
            _ctRepo = ct;
            _ctService = cts;
        }


        #region Counter Type
        [HttpGet("CounterType")]
        public async Task<IActionResult> GetCounterTypes()
        {
            var counterTypes = await _ctRepo.GetCounterTypes();
            return Ok(counterTypes);
        }

        [HttpGet("CounterTypeInfo")]
        public async Task<IActionResult> GetCounterTypeInfo(int counterTypeId)
        {
            var counterType = await _ctRepo.GetCounterTypeInfo(counterTypeId);
            return Ok(counterType);
        }

        [HttpGet("CounterTypeUpdateInfo")]
        public async Task<IActionResult> GetCounterTypeUpdateInfo(int counterTypeId)
        {
            var counterType = await _ctRepo.GetCounterTypeUpdateInfo(counterTypeId);
            return Ok(counterType);
        }

        [HttpPost("CounterType")]
        public async Task<IActionResult> CreatecounterType([FromBody]UpdateCounterType value)
        {
            await _ctService.CreateNewCounterType(value);
            return Ok();
        }

        [HttpPut("CounterType")]
        public async Task<IActionResult> UpdateCounterType([FromBody]UpdateCounterType value, int counterTypeId)
        {
            await _ctService.UpdateCounterType(counterTypeId, value);
            return Ok();
        }

        [HttpPut("CounterTypeRemoval")]
        public async Task<IActionResult> RemoveCounterType([FromBody]CounterTypeInfo value)
        {                  
            await _ctService.RemoveCounterType(value.CounterTypeId);
            return Ok();
        }  
        #endregion

        #region Counter

        [HttpGet("Counter")]
        public async Task<IActionResult> GetCounters()
        {
            var counters = await _cRepo.GetAll();
            return Ok(counters);
        }

        [HttpGet("CounterUpdateInfo")]
        public async Task<IActionResult> GetCounterUpdateInfo(int counterId)
        {
            var counter = await _cRepo.UpdateInfo(counterId);
            return Ok(counter);
        }

        [HttpGet("CounterInfo")]
        public async Task<IActionResult> GetCounterInfo(int counterId)
        {
            var counter = await _cRepo.Get(counterId);
            return Ok(counter);
        }

        [HttpPost("Counter")]
        public async Task<IActionResult> CreateCounter([FromBody]UpdateCounter value)
        {
            await _cService.Create(value);
            return Ok();
        }

        [HttpPut("Counter")]
        public async Task<IActionResult> UpdateCounter([FromBody]UpdateCounter value, int counterId)
        {
            await _cService.Update(value, counterId);
            return Ok();
        }

        [HttpPut("CounterRemoval")]
        public async Task<IActionResult> RemoveCounter([FromBody]CounterInfo value)
        {
            await _cService.Delete(value.CounterId);
            return Ok();
        }

        #endregion

    }
}
