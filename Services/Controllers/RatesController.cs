namespace Vueling.OTD.Services.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts;
    using Vueling.OTD.Contracts.DTOs;

    [ApiController]
    [Route("[controller]")]
    public class RatesController : ControllerBase, IRatesManager
    {
        private readonly IRatesManager ratesManager;

        public RatesController(IRatesManager ratesManager)
        {
            this.ratesManager = ratesManager;
        }

        [HttpGet("GetAllRates")]
        [ProducesResponseType(typeof(IEnumerable<RateDTO>), 200)]
        public Task<IEnumerable<RateDTO>> GetAllRatesAsync()
        {
            return this.ratesManager.GetAllRatesAsync();
        }
    }
}