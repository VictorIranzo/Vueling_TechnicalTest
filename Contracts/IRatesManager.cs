namespace Vueling.OTD.Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts.DTOs;

    public interface IRatesManager
    {
        Task<IEnumerable<RateDTO>> GetAllRatesAsync();
    }
}