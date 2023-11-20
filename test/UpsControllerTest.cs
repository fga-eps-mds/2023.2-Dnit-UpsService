using api.Ups;
using app.Controllers;
using app.Entidades;
using auth;
using Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stub;
using test.Fixtures;
using test.Stub;
using Xunit.Abstractions;

namespace test
{
    public class UpsControllerTest : AuthTest
    {
        readonly AppDbContext db;
        readonly UpsController upsController;

        public UpsControllerTest(ITestOutputHelper testOutputHelper, Base fixture) : base(testOutputHelper, fixture)
        {
            db = fixture.GetService<AppDbContext>(testOutputHelper)!;
            upsController = fixture.GetService<UpsController>(testOutputHelper)!;

            AutenticarUsuario(upsController);
        }

        [Fact]
        public async Task CalcularUpsSinistrosAsync_QuandoTemSinistrosCadastros_RetornaOk()
        {
            db.PopulaSinistros(5);

            var resultado = await upsController.CalcularUpsSinistrosAsync();

            Assert.IsType<OkResult>(resultado);
        }

        [Fact]
        public async Task CalcularUpsSinistrosAsync_QuandoNaoTiverPermissao_DeveBloquear()
        {
            db.PopulaSinistros(5);

            AutenticarUsuario(upsController, permissoes: new());
            await Assert.ThrowsAsync<AuthForbiddenException>(async () => await upsController.CalcularUpsSinistrosAsync());
        }

        [Fact]
        public async Task CalcularUpsEscolaAsync_QuandoNaoTiverPermissao_DeveBloquear()
        {
            AutenticarUsuario(upsController, permissoes: new());

            await Assert.ThrowsAsync<AuthForbiddenException>(
                async () => await upsController.CalcularUpsEscolaAsync(
                    new Escola { Latitude = 15.3, Longitude = 1.0 },
                    double.PositiveInfinity
                ));
        }

        [Fact]
        public async Task CalcularUpsEscolaAsync_QuandoTemSinistrosCadastros_RetornaUpsDetalhado()
        {
            var sinistros = new Sinistro[]{
                SinistroStub.Ups1(), SinistroStub.Ups4(),
                SinistroStub.Ups6(), SinistroStub.Ups13() };
            for (int i = 0; i < sinistros.Length; i++)
                sinistros[i].CalcularUps();
            db.Sinistros.AddRange(sinistros);
            db.SaveChanges();

            var resultado = await upsController.CalcularUpsEscolaAsync(
                new Escola { Latitude = 15.3, Longitude = 1.0 },
                double.PositiveInfinity
                );

            Assert.IsType<OkObjectResult>(resultado);
            var ups = ((resultado as OkObjectResult)?.Value as UpsDetalhado)!;

            Assert.Equal(24, ups.UpsGeral);
        }

        [Fact]
        public async Task CalcularUpsEscolasAsync_QuandoBancoDeDadosEmMemoria_LancaExcecao()
        {
            var escolas = new Escola[] { new() { Latitude = 1.1, Longitude = 1.2 } };
            var filtro = new CalcularUpsEscolasFiltro();

            await Assert.ThrowsAsync<NotSupportedException>(async () =>
                await upsController.CalcularUpsEscolasAsync(escolas, filtro));
        }

        internal new void Dispose()
        {
            db.Clear();
        }
    }
}