namespace Vueling.OTD.Logic.Tests.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts;
    using Vueling.OTD.Contracts.DTOs;
    using Vueling.OTD.Contracts.Enumerations;
    using Vueling.OTD.Logic.Clients;
    using Vueling.OTD.Logic.Tests.Mocks;
    using Vueling.OTD.Persistence;
    using Vueling.OTD.Persistence.Entities;

    [TestClass]
    public class RatesTests
    {
        /// <summary>
        ///     The method <see cref="IRatesManager.GetAllRatesAsync" />, when the server fails,
        ///     returns the rates stored at the database.
        /// </summary>
        /// <returns> A task that enables this method to be awaited. </returns>
        [TestMethod]
        public async Task GetAllRatesAsync_ServerFails_ReturnsPersistedRates()
        {
            // Arrange.
            void AdditionalRegistrations(IServiceCollection services)
            {
                APIClientMock aPIClientMock = new APIClientMock().MockGetRates(() => throw new ApiException());
                ClientFactoryMock clientFactoryMock = new ClientFactoryMock()
                    .MockGetClient(aPIClientMock);

                services.Remove(services.First(s => s.ServiceType == typeof(IClientFactory)));

                services.AddSingleton<IClientFactory>((s) => clientFactoryMock.Object);
            }

            IServiceProvider serviceProvider = TestsServiceProvider.GetServiceProvider(AdditionalRegistrations);
            IServiceProvider arrangeServiceProvder = serviceProvider.CreateScope().ServiceProvider;

            using (Context context = arrangeServiceProvder.GetService<Context>())
            {
                context.Database.EnsureCreated();

                context.Rates.Add(new Rate()
                {
                    Id = Guid.NewGuid(),
                    FromCurrency = Currency.Euro,
                    ToCurrency = Currency.UnitedStatesDollar,
                    Ratio = 1.1,
                });

                context.SaveChanges();
            }

            // Act.
            IServiceProvider actServiceProvder = serviceProvider.CreateScope().ServiceProvider;
            IRatesManager ratesManager = actServiceProvder.GetService<IRatesManager>();

            IEnumerable<RateDTO> result = await ratesManager.GetAllRatesAsync().ConfigureAwait(false);

            // Assert.
            IEnumerable<RateDTO> expectedResult = new List<RateDTO>()
            {
                new RateDTO()
                {
                    FromCurrency = Currency.Euro,
                    ToCurrency = Currency.UnitedStatesDollar,
                    Rate = 1.1,
                }
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        /// <summary>
        ///     The method <see cref="IRatesManager.GetAllRatesAsync" />, when the server returns a
        ///     result, returns them and persists them at database.
        /// </summary>
        /// <returns> A task that enables this method to be awaited. </returns>
        [TestMethod]
        public async Task GetAllRatesAsync_ServerSucceds_ReturnsRatesAndPersistsThem()
        {
            // Arrange.
            void AdditionalRegistrations(IServiceCollection services)
            {
                APIClientMock aPIClientMock = new APIClientMock().MockGetRates(() => new List<APIRate>()
                {
                    new APIRate()
                    {
                        From = "EUR",
                        To = "USD",
                        Rate = "1.10",
                    },
                });

                ClientFactoryMock clientFactoryMock = new ClientFactoryMock()
                    .MockGetClient(aPIClientMock);

                services.Remove(services.First(s => s.ServiceType == typeof(IClientFactory)));

                services.AddSingleton<IClientFactory>((s) => clientFactoryMock.Object);
            }

            IServiceProvider serviceProvider = TestsServiceProvider.GetServiceProvider(AdditionalRegistrations);
            IServiceProvider arrangeServiceProvder = serviceProvider.CreateScope().ServiceProvider;

            using (Context context = arrangeServiceProvder.GetService<Context>())
            {
                context.Database.EnsureCreated();

                context.SaveChanges();
            }

            // Act.
            IServiceProvider actServiceProvder = serviceProvider.CreateScope().ServiceProvider;
            IRatesManager ratesManager = actServiceProvder.GetService<IRatesManager>();

            IEnumerable<RateDTO> result = await ratesManager.GetAllRatesAsync().ConfigureAwait(false);

            // Assert.
            IServiceProvider assertServiceProvder = serviceProvider.CreateScope().ServiceProvider;

            IEnumerable<RateDTO> expectedResult = new List<RateDTO>()
            {
                new RateDTO()
                {
                    FromCurrency = Currency.Euro,
                    ToCurrency = Currency.UnitedStatesDollar,
                    Rate = 1.1,
                }
            };

            result.Should().BeEquivalentTo(expectedResult);

            using (Context context = assertServiceProvder.GetService<Context>())
            {
                IEnumerable<Rate> expectedRates = new List<Rate>()
                {
                    new Rate()
                    {
                        FromCurrency = Currency.Euro,
                        ToCurrency = Currency.UnitedStatesDollar,
                        Ratio = 1.1,
                    }
                };

                context.Rates.Should().BeEquivalentTo(expectedRates,
                    options => options.Excluding((member) => member.Path.EndsWith(nameof(EntityBase.Id))));
            }
        }
    }
}