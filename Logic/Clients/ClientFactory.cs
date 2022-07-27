using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Vueling.OTD.Logic.Clients
{
    internal class ClientFactory : IClientFactory
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly APIClientOptions options;

        public ClientFactory(IHttpClientFactory httpClientFactory, IOptions<APIClientOptions> options)
        {
            this.httpClientFactory = httpClientFactory;
            this.options = options.Value;
        }

        public APIClient GetClient()
        {
            return new APIClient(baseUrl: this.options.ServerUri, this.httpClientFactory.CreateClient("default"));
        }
    }
}