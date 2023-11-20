using api;
using api.Escolas;
using Entidades;

namespace Repositorio.Interfaces
{
    public interface ISinistroRepositorio
    {
        public Sinistro Criar(SinistroDTO sinistro);
        public Task<ListaPaginada<Sinistro>> ListarPaginadaAsync(PesquisaSinistroFiltro filtro);
        public Task<IEnumerable<Sinistro>> ObterTodosAsync();
        public Task<List<Sinistro>> ObterAPartirDoAnoDentroDeRaioAsync(Escola escola, double raioKm, uint ano);
    }
}