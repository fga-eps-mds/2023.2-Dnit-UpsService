using app.DI;
using Microsoft.Extensions.Options;
using Service.Interfaces;

namespace test.Mock
{
    public class EscolaServiceFake : IEscolaService
    {
        public EscolaServiceFake(HttpClient _, IOptions<EscolaServiceConfig> ___)
        {
        }

        public async Task CalcularNovoRanque()
        {
            await Task.Run(() => { });
        }
    }
}