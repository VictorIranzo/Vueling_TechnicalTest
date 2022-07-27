namespace Vueling.OTD.Logic
{
    using Microsoft.EntityFrameworkCore;
    using Polly;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts;
    using Vueling.OTD.Contracts.DTOs;
    using Vueling.OTD.Logic.Clients;
    using Vueling.OTD.Logic.Converters;
    using Vueling.OTD.Persistence.Entities;

    internal class RatesManager : IRatesManager
    {
        private readonly Persistence.Context context;
        private readonly IConverter<APIRate, Rate> apiRatesToRateConverter;
        private readonly IConverter<Rate, RateDTO> ratesToRateDtoConverter;
        private readonly PollyPolicies pollyPolicies;
        private readonly IClientFactory clientFactory;

        public RatesManager(
            Persistence.Context context,
            IConverter<APIRate, Rate> apiRatesToRateConverter,
            IConverter<Rate, RateDTO> ratesToRateDtoConverter,
            PollyPolicies pollyPolicies,
            IClientFactory clientFactory)
        {
            this.context = context;
            this.apiRatesToRateConverter = apiRatesToRateConverter;
            this.ratesToRateDtoConverter = ratesToRateDtoConverter;
            this.pollyPolicies = pollyPolicies;
            this.clientFactory = clientFactory;
        }

        public async Task<IEnumerable<RateDTO>> GetAllRatesAsync()
        {
            APIClient apiClient = this.clientFactory.GetClient();

            PolicyResult<ICollection<APIRate>> pollyResultFromClient = 
                await this.pollyPolicies.RetryPolicy.ExecuteAndCaptureAsync(apiClient.GetRatesAsync).ConfigureAwait(false);

            if (pollyResultFromClient.Outcome == OutcomeType.Successful)
            {
                // Updates the existing values at the database and returns them.
                IEnumerable<Rate> newRates = this.apiRatesToRateConverter.Convert(pollyResultFromClient.Result);

                this.UpdateRatesAtDatabase(newRates);

                return this.ratesToRateDtoConverter.Convert(newRates);
            }
            else
            {
                // If an error occurs invoking the client after the Polly retries,
                // returns the values stored at the database.
                return this.ratesToRateDtoConverter.Convert(this.context.Rates);
            }
        }

        private void UpdateRatesAtDatabase(IEnumerable<Rate> newRates)
        {
            this.context.Rates.RemoveAllRows();
            this.context.Rates.AddRange(newRates);

            this.context.SaveChanges();
        }
    }
}