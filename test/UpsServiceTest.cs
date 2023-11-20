using api.Ups;
using app.Entidades;
using Entidades;
using Microsoft.EntityFrameworkCore;
using Moq;
using Repositorio.Interfaces;
using Service;

namespace test
{
    public class UpsServiceTest
    {
        private readonly DbContextOptions<AppDbContext> options = new();
        private readonly Mock<AppDbContext> mockDb;
        private readonly Mock<ISinistroRepositorio> sinistroRepositorioMock;
        private readonly UpsService upsService;
        private readonly DateTime dataAtual = DateTime.Now;

        public UpsServiceTest()
        {
            mockDb = new(options);
            sinistroRepositorioMock = new();
            upsService = new UpsService(sinistroRepositorioMock.Object, mockDb.Object);
        }

        [Fact]
        public void CalcularDistancia_QuandoLatLongForPassada_DeveCalcularDistancia()
        {
            double distancia = 1878.5472845077677;
            double lat1 = -19.83990774;
            double long1 = -43.87701717;
            double lat2 = -3.7437141;
            double long2 = -38.6158061;

            double distanciaCalculada = upsService.CalcularDistancia(lat1, long1, lat2, long2);

            Assert.Equal(distancia, distanciaCalculada);
        }

        [Fact]
        public void CalcularDistancia_QuandoLatLongForPassadaIgual_DistanciaDeveSerZero()
        {
            double distancia = 0;
            double lat1 = -19.83990774;
            double long1 = -43.87701717;
            double lat2 = -19.83990774;
            double long2 = -43.87701717;

            double distanciaCalculada = upsService.CalcularDistancia(lat1, long1, lat2, long2);

            Assert.Equal(distancia, distanciaCalculada);
        }

        [Fact]
        public async Task CalcularUpsMuitasEscolasAsync_QuandoNormal_SomaUpsDeTodosSinistrosAoRedor()
        {
            var escolas = GeraEscolas(1, lat: 1.3, lon: 1.2);

            sinistroRepositorioMock
                .Setup(x => x.ObterAPartirDoAnoDentroDeRaioAsync(It.IsAny<Escola>(), It.IsAny<double>(), It.IsAny<uint>()))
                .ReturnsAsync(new List<Sinistro>() {
                    new() {Longitude = 1.3, Latitude = 1.2, Ups = 6},
                    new() {Longitude = 1.3, Latitude = 1.2, Ups = 6},
                });

            var filtro = new CalcularUpsEscolasFiltro();
            var upss = await upsService.CalcularUpsMuitasEscolasAsync(escolas, filtro);

            Assert.Single(upss);
            Assert.Equal(12, upss[0]);
        }

        [Fact]
        public async Task CalcularUpsMuitasEscolasAsync_QuandoPassadoAnoMuitoAntigo_UsaAnoAtualMenos5EhUsadoComoFiltro()
        {
            var escolas = GeraEscolas(1, lat: 1.3, lon: 1.2);

            var dataMuitoAntiga = DateTime.Now.AddYears(-8);
            sinistroRepositorioMock
                .Setup(x => x.ObterAPartirDoAnoDentroDeRaioAsync(It.IsAny<Escola>(), It.IsAny<double>(), It.IsAny<uint>()))
                .ReturnsAsync(new List<Sinistro>() {
                    new() { Longitude = 1.3, Latitude = 1.2}});

            var upss = await upsService.CalcularUpsMuitasEscolasAsync(
                escolas, new CalcularUpsEscolasFiltro() { DesdeAno = (uint)dataMuitoAntiga.Year });

            var anoEsperadoComoLimite = (uint)(dataAtual.Year - 5);
            sinistroRepositorioMock
                .Verify(x =>
                    x.ObterAPartirDoAnoDentroDeRaioAsync(It.IsAny<Escola>(), It.IsAny<double>(), anoEsperadoComoLimite),
                    Times.Once());
        }

        [Fact]
        public async Task CalcularUpsMuitasEscolasAsync_QuandoRaioNaoFornecido_2UsadoComoPadrao()
        {
            var escolas = GeraEscolas(1, lat: 1.3, lon: 1.2);
            var dataMuitoAntiga = DateTime.Now.AddYears(-8);
            sinistroRepositorioMock
                .Setup(x => x.ObterAPartirDoAnoDentroDeRaioAsync(It.IsAny<Escola>(), It.IsAny<double>(), It.IsAny<uint>()))
                .ReturnsAsync(new List<Sinistro>() { new() { Longitude = 1.3, Latitude = 1.2 } });

            var upss = await upsService.CalcularUpsMuitasEscolasAsync(
                escolas, new CalcularUpsEscolasFiltro() { DesdeAno = (uint)dataMuitoAntiga.Year });

            sinistroRepositorioMock
                .Verify(x =>
                    x.ObterAPartirDoAnoDentroDeRaioAsync(It.IsAny<Escola>(), 2.0, It.IsAny<uint>()),
                    Times.Once());
        }

        private Escola[] GeraEscolas(uint quantidade, double lat, double lon)
        {
            var escolas = new Escola[quantidade];
            for (int i = 0; i < quantidade; i++)
            {
                escolas[i] = new()
                {
                    Latitude = lat,
                    Longitude = lon,
                };
            }
            return escolas;
        }
    }
}