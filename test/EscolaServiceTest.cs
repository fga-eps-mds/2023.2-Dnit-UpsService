using app.DI;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Service;

namespace test
{
    public class EscolaServiceTest
    {
        private readonly EscolaService service;
        private readonly MockHttpMessageHandler mockHttp;
        private readonly EscolaServiceConfig config = new() 
            { Host = "http://localhost" };

        public EscolaServiceTest()
        {
            mockHttp = new MockHttpMessageHandler();
            service = new EscolaService(
                mockHttp.ToHttpClient(),
                Options.Create(config));
        }

        [Fact]
        public async Task CalcularNovoRanque()
        {
            var requisicao = mockHttp
                .When(config.Host + "/api/ranque/escolas/novo")
                .Respond("application/json", "");
            
            await service.CalcularNovoRanque();

            Assert.Equal(1, mockHttp.GetMatchCount(requisicao));
        }
    }
}