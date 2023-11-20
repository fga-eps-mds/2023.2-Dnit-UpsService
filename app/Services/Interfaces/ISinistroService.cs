using api;
using api.Escolas;
using Entidades;

namespace Service.Interfaces
{
    public interface ISinistroService
    {
        public bool SuperaTamanhoMaximo(MemoryStream planilha);
        public Task CadastrarSinistroViaPlanilha(MemoryStream planilha);
        public Task<ListaPaginada<Sinistro>> ListarPaginadaAsync(PesquisaSinistroFiltro filtro);
    }
}