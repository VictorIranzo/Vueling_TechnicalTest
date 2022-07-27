namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Vueling.OTD.Contracts;
    using Vueling.OTD.Contracts.DTOs;
    using Vueling.OTD.Contracts.Enumerations;
    using Vueling.OTD.Logic;
    using Vueling.OTD.Logic.Clients;
    using Vueling.OTD.Logic.Converters;
    using Vueling.OTD.Persistence.Entities;

    public static class LogicServiceCollectionExtensions
    {
        public static IServiceCollection AddLogicServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRatesManager, RatesManager>();
            services.AddScoped<ITransactionsManager, TransactionsManager>();

            services.AddSingleton<IConverter<string, Currency>, CurrencyConverter>();

            services.AddSingleton<IConverter<APIRate, Rate>, ApiRateToRateConverter>();
            services.AddSingleton< IConverter<Rate,RateDTO>, RateToRateDtoConverter>();

            services.AddSingleton<IConverter<APITransaction, Transaction>, ApiTransactionToTransactionConverter>();
            services.AddSingleton<IConverter<Transaction, TransactionDTO>, TransactionToTransactionDtoConverter>();

            services.AddSingleton<IClientFactory, ClientFactory>();
            services.Configure<APIClientOptions>(
                configuration.GetSection("Server"));

            services.AddSingleton<PollyPolicies>();
            services.Configure<PollyOptions>(
                configuration.GetSection("Polly"));

            services.AddHttpClient();

            return services;
        }
    }
}