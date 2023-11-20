using api;
using app.Entidades;
using Repositorio.Interfaces;
using Entidades;
using Microsoft.EntityFrameworkCore;
using api.Escolas;

namespace Repositorio
{
    public class SinistroRepositorio : ISinistroRepositorio
    {
        private readonly AppDbContext db;

        public SinistroRepositorio(AppDbContext db)
        {
            this.db = db;
        }

        public Sinistro Criar(SinistroDTO sinistro)
        {
            var sin = new Sinistro
            {
                Id = sinistro.Id,
                Uf = sinistro.Uf,
                Rodovia = sinistro.Rodovia,
                Km = sinistro.Km,
                Snv = sinistro.Snv,
                Sentido = sinistro.Sentido,
                Solo = sinistro.Solo,
                Data = sinistro.Data,
                Tipo = sinistro.Tipo,
                Causa = sinistro.Causa,
                Gravidade = sinistro.Gravidade,
                Feridos = sinistro.Feridos,
                Mortos = sinistro.Mortos,
                Latitude = sinistro.Latitude,
                Longitude = sinistro.Longitude,
            };
            sin.CalcularUps();
            db.Sinistros.Add(sin);
            return sin;
        }

        public async Task<ListaPaginada<Sinistro>> ListarPaginadaAsync(PesquisaSinistroFiltro filtro)
        {
            var total = await db.Sinistros.CountAsync();
            var sinistros = await db.Sinistros
                .OrderBy(s => s.Id)
                .Skip(filtro.ItemsPorPagina * (filtro.Pagina - 1))
                .Take(filtro.ItemsPorPagina)
                .ToListAsync();
            return new ListaPaginada<Sinistro>(sinistros, filtro.Pagina, filtro.ItemsPorPagina, total);
        }

        public async Task<IEnumerable<Sinistro>> ObterTodosAsync()
        {
            var sinistros = await db.Sinistros.ToListAsync();
            return sinistros;
        }

        public async Task<List<Sinistro>> ObterAPartirDoAnoDentroDeRaioAsync(Escola escola, double raioKm, uint ano)
        {
            var query = from s in db.Sinistros
                        where s.DataUtc.Year >= ano
                        where db.CalcularDistancia(s.Latitude, s.Longitude, escola.Latitude, escola.Longitude) <= raioKm
                        select s;
            return await query.ToListAsync();
        }
    }
}
