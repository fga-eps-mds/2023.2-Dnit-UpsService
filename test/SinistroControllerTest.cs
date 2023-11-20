using app.Controllers;
using auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using test.Fixtures;
using Xunit.Abstractions;

namespace test
{
    public class SinistroControllerTest : AuthTest
    {
        readonly SinistroController sinistroController;
        readonly string caminhoStub = Path.Join("..", "..", "..", "..", "test", "Stub");

        public SinistroControllerTest(ITestOutputHelper testOutputHelper, Base fixture) : base(testOutputHelper, fixture)
        {
            sinistroController = fixture.GetService<SinistroController>(testOutputHelper)!;

            AutenticarUsuario(sinistroController);
        }

        [Fact]
        public async Task EnviarPlanilhaAsync_QuandoFormatoInvalido_RetornaBadRequest()
        {
            var caminhoArquivo = Path.Join(caminhoStub, "ExemploSin.csv");
            var conteudo = File.ReadAllBytes(caminhoArquivo);
            var memoryStream = new MemoryStream(File.ReadAllBytes(caminhoArquivo));
            var arquivo = new FormFile(memoryStream, 0, conteudo.Length, "planilha", "");
            arquivo.Headers = new HeaderDictionary();
            arquivo.Headers.ContentType = "application/json";

            var resultado = await sinistroController.EnviarPlanilhaAsync(arquivo);
            var message = (resultado as BadRequestObjectResult)?.Value as string;

            Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal("Formato deve ser CSV", message);
        }

        [Fact]
        public async Task EnviarPlanilhaAsync_QuandoNaoTiverPermissao_DeveBloquear()
        {
            var caminhoArquivo = Path.Join(caminhoStub, "ExemploSin.csv");
            var conteudo = File.ReadAllBytes(caminhoArquivo);
            var memoryStream = new MemoryStream(File.ReadAllBytes(caminhoArquivo));
            var arquivo = new FormFile(memoryStream, 0, conteudo.Length, "planilha", "");
            arquivo.Headers = new HeaderDictionary();
            arquivo.Headers.ContentType = "application/json";

            AutenticarUsuario(sinistroController, permissoes: new());
            await Assert.ThrowsAsync<AuthForbiddenException>(async () => await sinistroController.EnviarPlanilhaAsync(arquivo));
        }

        [Fact]
        public async Task EnviarPlanilhaAsync_QuandoArquivoMuitoGrande_RetornaBadRequest()
        {
            var caminhoArquivo = Path.Join(caminhoStub, "ExemploSinistroTamanhoMaximo.csv");
            var conteudo = File.ReadAllBytes(caminhoArquivo);
            var memoryStream = new MemoryStream(conteudo);
            var arquivo = new FormFile(memoryStream, 0, conteudo.Length, "planilha", "planilha.csv");
            arquivo.Headers = new HeaderDictionary();
            arquivo.Headers.ContentType = "text/csv";

            var resultado = await sinistroController.EnviarPlanilhaAsync(arquivo);
            var message = (resultado as BadRequestObjectResult)?.Value as string;

            Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal("Tamanho máximo de arquivo ultrapassado", message);
        }

        [Fact]
        public async Task EnviarPlanilhaAsync_QuandoArquivoVazio_RetornaBadRequest()
        {
            var conteudo = Array.Empty<byte>();
            var memoryStream = new MemoryStream(conteudo);
            var arquivo = new FormFile(memoryStream, 0, conteudo.Length, "planilha", "");
            arquivo.Headers = new HeaderDictionary();
            arquivo.Headers.ContentType = "text/csv";

            var resultado = await sinistroController.EnviarPlanilhaAsync(arquivo);
            var message = (resultado as BadRequestObjectResult)?.Value as string;

            Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal("Arquivo vazio", message);
        }

        [Fact]
        public async Task EnviarPlanilhaAsync_QuandoArquivoNormal_RetornaOk()
        {
            var caminhoArquivo = Path.Join(caminhoStub, "ExemploSin.csv");
            var conteudo = File.ReadAllBytes(caminhoArquivo);
            var memoryStream = new MemoryStream(conteudo);
            var arquivo = new FormFile(memoryStream, 0, conteudo.Length, "planilha", "planilha.csv");
            arquivo.Headers = new HeaderDictionary();
            arquivo.Headers.ContentType = "text/csv";

            var resultado = await sinistroController.EnviarPlanilhaAsync(arquivo);

            Assert.IsType<OkResult>(resultado);
        }
    }
}