using api.Ups;
using Entidades;

namespace Service.Interfaces
{
    public interface IUpsService
    {
        public Task CalcularUpsEmMassaAsync();
        public Task<UpsDetalhado> CalcularUpsEscolaAsync(Escola escola, double raioKm);
        public double CalcularDistancia(double lat1, double long1, double lat2, double long2);

        public Task<int[]> CalcularUpsMuitasEscolasAsync(Escola[] escolas, CalcularUpsEscolasFiltro filtro);
    }
}
