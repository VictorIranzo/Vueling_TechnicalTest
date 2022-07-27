namespace Vueling.OTD.Logic.Tests.Mocks
{
    using Moq;
    using System.Collections.Generic;
    using Vueling.OTD.Contracts.DTOs;

    internal class RatesManagerMock : Mock<RatesManagerFake>
    {
        public RatesManagerMock()
        {
            this.CallBase = true;
        }

        public RatesManagerMock MockGetAllRates(IEnumerable<RateDTO> outputRates)
        {
            this.Setup(s => s.GetAllRatesAsync()).ReturnsAsync(outputRates);

            return this;
        }
    }
}