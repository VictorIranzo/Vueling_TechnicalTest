namespace Vueling.OTD.Logic.Tests.Mocks
{
    using Moq;
    using Vueling.OTD.Logic.Clients;

    internal class ClientFactoryMock : Mock<ClientFactoryFake>
    {
        public ClientFactoryMock()
        {
            this.CallBase = true;
        }

        public ClientFactoryMock MockGetClient(APIClientMock mockAPIClient)
        {
            this.Setup(s => s.GetClient()).Returns(mockAPIClient.Object);

            return this;
        }
    }
}