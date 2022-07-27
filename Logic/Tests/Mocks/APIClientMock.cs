namespace Vueling.OTD.Logic.Tests.Mocks
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using Vueling.OTD.Logic.Clients;

    public class APIClientMock : Mock<APIClient>
    {
        public APIClientMock()
        {
            this.CallBase = true;
        }

        public APIClientMock MockGetRates(Func<ICollection<APIRate>> returnRatesFunc)
        {
            this.Setup(s => s.GetRatesAsync()).ReturnsAsync(returnRatesFunc);

            return this;
        }
    }
}