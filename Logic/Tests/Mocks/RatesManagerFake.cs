namespace Vueling.OTD.Logic.Tests.Mocks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts;
    using Vueling.OTD.Contracts.DTOs;

    public class RatesManagerFake : IRatesManager
    {
        public virtual Task<IEnumerable<RateDTO>> GetAllRatesAsync()
        {
            return Task.FromResult(new List<RateDTO>().AsEnumerable());
        }
    }
}