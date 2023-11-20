using Moq;
using Service;
using Repositorio.Interfaces;
using app.Entidades;
using Microsoft.EntityFrameworkCore;
using api;
using Service.Interfaces;

namespace test.SinistroServiceTests
{
    public class SinistroServiceTest
    {
        private readonly DbContextOptions<AppDbContext> options = new();
        private readonly Mock<AppDbContext> mockDb;
        private readonly SinistroService sinistroService;
        private readonly Mock<ISinistroRepositorio> mockSinistroRepositorio;
        private readonly Mock<IEscolaService> mockEscolaService;
        private readonly string caminhoTests = Path.Join("..", "..", "..", "..", "test");
        private string caminhoDoArquivo;

        public SinistroServiceTest()
        {
            mockSinistroRepositorio = new();
            mockEscolaService = new();
            mockDb = new(options);

            sinistroService = new SinistroService(
                mockDb.Object,
                mockSinistroRepositorio.Object,
                mockEscolaService.Object);
            caminhoDoArquivo = Path.Join(caminhoTests, "Stub", "ExemploSin.csv");
        }

        [Fact]
        public async Task CadastrarSinistroViaPlanilha_QuandoPlanilhaForPassadaENaoTiverDado_NaoDevePassarPeloRepositorio()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async ()
                => await sinistroService.CadastrarSinistroViaPlanilha(null));
        }

        [Fact]
        public async Task CadastrarSinistroViaPlanilha_QuandoForChamado_DeveChamarORepositorio()
        {
            var memoryStream = new MemoryStream(File.ReadAllBytes(caminhoDoArquivo));

            await sinistroService.CadastrarSinistroViaPlanilha(memoryStream);
            mockSinistroRepositorio.Verify(mock => mock.Criar(It.IsAny<SinistroDTO>()), Times.Exactly(3));
        }

        [Fact]
        public async Task CadastrarSinistroViaPlanilha_QuandForChamado_DeveCadastrarSinistros()
        {
            var memoryStream = new MemoryStream(File.ReadAllBytes(caminhoDoArquivo));

            await sinistroService.CadastrarSinistroViaPlanilha(memoryStream);
            mockSinistroRepositorio.Verify(mock => mock.Criar(It.IsAny<SinistroDTO>()), Times.Exactly(3));
        }

        [Fact]
        public void SuperaTamanhoMaximo_QuandoPlanilhaComTamanhoMaiorQueOMaximoForPassada_DeveRetornarTrue()
        {
            caminhoDoArquivo = Path.Join(caminhoTests, "Stub", "planilhaExemploSinistro.csv");

            var memoryStream = new MemoryStream(File.ReadAllBytes(caminhoDoArquivo));
            bool resultado = sinistroService.SuperaTamanhoMaximo(memoryStream);
            Assert.True(resultado);
        }
    }
}