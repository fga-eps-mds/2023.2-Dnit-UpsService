using Service.Interfaces;
using Repositorio.Interfaces;
using Microsoft.VisualBasic.FileIO;
using Entidades;
using app.Entidades;
using Microsoft.EntityFrameworkCore;
using api;
using api.Escolas;
using app.Services;

namespace Service
{
    public class SinistroService : ISinistroService
    {
        private readonly ISinistroRepositorio sinistroRepositorio;
        private readonly AppDbContext db;
        private readonly IEscolaService escolaService;

        public SinistroService(
            AppDbContext db, 
            ISinistroRepositorio sinistroRepositorio, 
            IEscolaService escolaService
        )
        {
            this.db = db;
            this.sinistroRepositorio = sinistroRepositorio;
            this.escolaService = escolaService;
        }

        public bool SuperaTamanhoMaximo(MemoryStream planilha)
        {
            using (var reader = new StreamReader(planilha))
            {
                int tamanhoMax = 5000;
                int quantidadeSinistros = -1;

                while (reader.ReadLine() != null) { quantidadeSinistros++; }

                return quantidadeSinistros > tamanhoMax;
            }
        }

        public async Task CadastrarSinistroViaPlanilha(MemoryStream planilha)
        {
            int numeroLinha = 2;

            using (var reader = new StreamReader(planilha))
            {
                using (var parser = new TextFieldParser(reader))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    bool primeiralinha = false;

                    SinistroDTO sinistro;
                    string[] linha = Array.Empty<string>();
                    while (!parser.EndOfData)
                    {
                        try
                        {
                            linha = parser.ReadFields() ?? Array.Empty<string>();
                            if (!primeiralinha)
                            {
                                primeiralinha = true;
                                continue;
                            }
                            sinistro = new()
                            {
                                Id = int.Parse(linha[0]),
                                Uf = Enum.Parse<UF>(linha[1]),
                                Rodovia = int.Parse(linha[2]),
                                Km = double.Parse(linha[3]),
                                Snv = linha[4],
                                Sentido = linha[5],
                                Solo = linha[6],
                                Data = DateTime.Parse(linha[7]),
                                Tipo = linha[8],
                                Causa = linha[9],
                                Gravidade = linha[10],
                                Feridos = int.Parse(linha[11]),
                                Mortos = int.Parse(linha[12]),
                                Latitude = double.Parse(linha[13]),
                                Longitude = double.Parse(linha[14])
                            };
                            sinistroRepositorio.Criar(sinistro);
                            db.SaveChanges();
                            numeroLinha++;
                        }
                        catch (FormatException ex)
                        {
                            throw new ApiException(ErrorCodes.PlanilhaFormatoIncompativel);
                        }
                        catch (DbUpdateException ex)
                        {
                            var mensagem = $"Dados já inseridos. Linha {numeroLinha}\n" +
                                $"Sinistro: {string.Join(';', linha)}";
                            throw new ApiException(ErrorCodes.DadosJaInseridos, mensagem);
                        }
                    }
                }
            }

            await escolaService.CalcularNovoRanque();
        }

        public async Task<ListaPaginada<Sinistro>> ListarPaginadaAsync(PesquisaSinistroFiltro filtro)
        {
            return await sinistroRepositorio.ListarPaginadaAsync(filtro);
        }
    }
}