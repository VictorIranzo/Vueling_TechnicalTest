namespace Vueling.OTD.Logic.Tests
{
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using Vueling.OTD.Persistence;

    internal static class TestsServiceProvider
    {
        public static IServiceProvider GetServiceProvider(Action<IServiceCollection> additionalRegistrations = null)
        {
            ServiceCollection services = new ServiceCollection();

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "Server:ServerUri", "FakeUri" },
                { "Polly:NumberOfRetries", "0" },
            });

            IConfiguration configuration = configurationBuilder.Build();

            services.AddLogicServices(configuration);
            services.AddLogging();

            SqliteConnection keepAliveConnection = new SqliteConnection("DataSource=:memory:");

            keepAliveConnection.Open();

            services.AddDbContext<Context>(options =>
            {
                options.UseSqlite(keepAliveConnection);
            });

            additionalRegistrations?.Invoke(services);

            return services.BuildServiceProvider();
        }
    }
}