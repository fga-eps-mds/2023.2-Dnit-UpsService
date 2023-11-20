using api.Escolas;
using api.Ups;
using app.Entidades;
using Entidades;
using Repositorio.Interfaces;
using Service.Interfaces;

namespace Service
{
    public class UpsService : IUpsService
    {
        private readonly ISinistroRepositorio sinistroRepositorio;
        private readonly AppDbContext db;
        private const double raioTerraEmKm = 6371.0;

        public UpsService(ISinistroRepositorio sinistroRepositorio, AppDbContext db)
        {
            this.sinistroRepositorio = sinistroRepositorio;
            this.db = db;
        }

        public async Task CalcularUpsEmMassaAsyncAntigo()
        {
            var sinistros = await sinistroRepositorio.ObterTodosAsync();
            foreach (var sinistro in sinistros)
            {
                sinistro.CalcularUps();
            }
            await db.SaveChangesAsync();
        }

        public async Task CalcularUpsEmMassaAsync()
        {
            var filtro = new PesquisaSinistroFiltro
            {
                Pagina = 1,
                ItemsPorPagina = 5000
            };
            var itemsProcessados = 0;
            var totalItems = db.Sinistros.Count();
            var totalPaginas = Math.Ceiling((float)totalItems / filtro.ItemsPorPagina) + 1;
            do
            {
                // BackgroundJob.Enqueue(() => CalcularUpsDeUmaListaDeSinistros(filtro));
                await CalcularUpsDeUmaListaDeSinistros(filtro);
                filtro.Pagina++;
                itemsProcessados += filtro.ItemsPorPagina;
            } while (filtro.Pagina != totalPaginas);
        }

        public async Task CalcularUpsDeUmaListaDeSinistros(PesquisaSinistroFiltro filtro)
        {
            var lista = await sinistroRepositorio.ListarPaginadaAsync(filtro);
            foreach (var sinistro in lista.Items!)
                sinistro.CalcularUps();
            await db.SaveChangesAsync();
        }

        public async Task<UpsDetalhado> CalcularUpsEscolaAsync(Escola escola, double raioKm)
        {
            var sinistros = await sinistroRepositorio.ObterTodosAsync();
            var upsDetalhado = new UpsDetalhado();

            Dictionary<int, int> upsPorAno = new()
            {
                { 2022, 0 },
                { 2021, 0 },
                { 2020, 0 },
                { 2019, 0 },
                { 2018, 0 }
            };

            foreach (var sinistro in sinistros)
            {
                if (CalcularDistancia(sinistro.Latitude, sinistro.Longitude, escola.Latitude, escola.Longitude) <= raioKm)
                {
                    if (upsPorAno.ContainsKey(sinistro.Data.Year))
                    {
                        upsPorAno[sinistro.Data.Year] += sinistro.Ups ?? 0;
                    }
                    else
                    {
                        upsPorAno.Add(sinistro.Data.Year, sinistro.Ups ?? 0);
                    }
                }
            }

            upsDetalhado.Ups2022 = upsPorAno[2022];
            upsDetalhado.Ups2021 = upsPorAno[2021];
            upsDetalhado.Ups2020 = upsPorAno[2020];
            upsDetalhado.Ups2019 = upsPorAno[2019];
            upsDetalhado.Ups2018 = upsPorAno[2018];

            upsDetalhado.CalcularUpsGeral();
            return upsDetalhado;
        }

        public static double ConverterParaRadianos(double grau)
        {
            return grau * Math.PI / 180.0;
        }

        public double CalcularDistancia(double lat1, double long1, double lat2, double long2)
        {
            var diferencaLatitude = ConverterParaRadianos(lat2 - lat1);
            var diferencaLongitude = ConverterParaRadianos(long2 - long1);

            var primeiraParteFormula = Math.Sin(diferencaLatitude / 2) * Math.Sin(diferencaLatitude / 2) +
                    Math.Cos(ConverterParaRadianos(lat1)) * Math.Cos(ConverterParaRadianos(lat2)) *
                    Math.Sin(diferencaLongitude / 2) * Math.Sin(diferencaLongitude / 2);

            var resultadoFormula = 2 * Math.Atan2(Math.Sqrt(primeiraParteFormula), Math.Sqrt(1 - primeiraParteFormula));

            var distance = raioTerraEmKm * resultadoFormula;

            return distance;
        }

        public async Task<int[]> CalcularUpsMuitasEscolasAsync(Escola[] escolas, CalcularUpsEscolasFiltro filtro)
        {
            uint limite = (uint)DateTime.Now.AddYears(-5).Year;
            uint ano;
            if (filtro.DesdeAno == null)
                ano = limite;
            else if (filtro.DesdeAno <= limite || filtro.DesdeAno < 0)
                ano = limite;
            else
                ano = (uint)filtro.DesdeAno;

            double raioKm = filtro.RaioKm ?? 2;
            var upss = new int[escolas.Length];

            // FIXME: Seria melhor que fosse feita apenas uma query para
            // todas as escolas.
            for (int i = 0; i < escolas.Length; i++)
            {
                var sinistros = await sinistroRepositorio
                    .ObterAPartirDoAnoDentroDeRaioAsync(escolas[i], raioKm, ano);
                foreach (var s in sinistros)
                    upss[i] += s.Ups ?? 0;
            }

            return upss;
        }
    }
}
