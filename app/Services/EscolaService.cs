using app.DI;
using Microsoft.Extensions.Options;
using Service.Interfaces;

namespace Service
{
    public class EscolaService : IEscolaService
    {
        private readonly HttpClient httpClient;
        private readonly string host;
        private readonly string calcularRanqueEndpoint = "/api/ranque/escolas/novo";

        public EscolaService(HttpClient _httpClient, IOptions<EscolaServiceConfig> config)
        {
            host = config.Value.Host;
            httpClient = _httpClient;
            httpClient.BaseAddress = new Uri(host);
        }

        public async Task CalcularNovoRanque()
        {
            await httpClient.PostAsync(calcularRanqueEndpoint, null);
        }
    }
}