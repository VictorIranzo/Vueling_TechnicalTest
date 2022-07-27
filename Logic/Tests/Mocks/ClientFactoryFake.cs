namespace Vueling.OTD.Logic.Tests.Mocks
{
    using System.Net.Http;
    using Vueling.OTD.Logic.Clients;

    public class ClientFactoryFake : IClientFactory
    {
        public virtual APIClient GetClient()
        {
            return new APIClient(baseUrl: string.Empty, httpClient: new HttpClient());
        }
    }
}