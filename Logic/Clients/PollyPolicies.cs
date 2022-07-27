namespace Vueling.OTD.Logic.Clients
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.Retry;
    using System;

    internal class PollyPolicies
    {
        private readonly PollyOptions pollyOptions;
        private readonly ILogger<PollyPolicies> logger;

        public PollyPolicies(IOptions<PollyOptions> pollyOptions, ILogger<PollyPolicies> logger)
        {
            this.pollyOptions = pollyOptions.Value;
            this.logger = logger;
        }

        public AsyncRetryPolicy RetryPolicy => Policy
            .Handle<ApiException>()
            .WaitAndRetryAsync(
                retryCount: this.pollyOptions.NumberOfRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryNumber, context) =>
                {
                    this.logger.LogError($"Retry nr. {retryNumber}. Exception {exception.Message}");
                });
    }
}