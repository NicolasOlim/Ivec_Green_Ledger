using ApiIveco.Models;
using ApiIveco.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // <-- BIBLIOTECA DE LOGS ADICIONADA
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiIveco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DadosController : ControllerBase
    {
        private readonly DadosService _dadosService;
        private readonly ILogger<DadosController> _logger; // <-- SERVIÇO DE LOG DECLARADO

        public DadosController(DadosService dadosService, ILogger<DadosController> logger)
        {
            _dadosService = dadosService;
            _logger = logger; // <-- INJEÇÃO DE DEPENDÊNCIA
        }

        /// <summary>
        /// Retorna a lista completa de veiculos cadastrados no banco de dados.
        /// </summary>
        /// <returns>Uma lista contendo todos os veiculos.</returns>
        /// <response code="200">Retorna a lista de veiculos com sucesso.</response>
        /// <response code="400">Se houver algum problema com os parâmetros de consulta.</response>
        /// <response code="404">Se a coleção de veiculos não for encontrada.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor (ex: falha no Firebase).</response>
        [Tags("Veículos")]
        [HttpGet("veiculos")]
        public async Task<IActionResult> GetVeiculos()
        {
            _logger.LogInformation("[GET] Requisição recebida para listar todos os veículos.");
            try
            {
                var veiculos = await _dadosService.ListarVeiculo();
                _logger.LogInformation($"[GET] Sucesso: {veiculos.Count} veículos retornados.");
                return Ok(veiculos);
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao listar veículos"); }
        }

        /// <summary>
        /// Busca um veiculo específico pelo seu VIN.
        /// </summary>
        /// <param name="vin">O identificador único do veiculo (String).</param>
        /// <returns>Os detalhes do veiculo correspondente ao ID informado.</returns>
        /// <response code="200">Retorna o veiculo encontrado com sucesso.</response>
        /// <response code="400">Se o ID fornecido for nulo ou vazio.</response>
        /// <response code="404">Se nenhum veiculo for encontrado com o ID especificado.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor.</response>
        [Tags("Veículos")]
        [HttpGet("veiculos/{vin}")]
        public async Task<IActionResult> GetVeiculoByVin(string vin)
        {
            _logger.LogInformation($"[GET] Buscando veículo com VIN: {vin}");
            try
            {
                if (string.IsNullOrWhiteSpace(vin))
                {
                    _logger.LogWarning("[GET] Falha na busca: VIN não fornecido.");
                    return BadRequest(new { Erro = "VIN obrigatório", Mensagem = "O VIN não pode ser vazio." });
                }

                var veiculo = await _dadosService.ObterVeiculoPorVin(vin);
                if (veiculo == null)
                {
                    _logger.LogWarning($"[GET] Veículo não encontrado para o VIN: {vin}");
                    return NotFound(new { Erro = "Não encontrado", Mensagem = $"Nenhum veículo encontrado com o VIN: {vin}" });
                }

                _logger.LogInformation($"[GET] Veículo localizado: {vin}");
                return Ok(new { mensagem = "Veículo encontrado", veiculo });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao obter veículo"); }
        }

        /// <summary>
        /// Gera um relatório em PDF de todos os veículos cadastrados.
        /// </summary>
        /// <returns>Um arquivo PDF.</returns>
        [Tags("Veículos")]
        [HttpGet("relatorios/veiculos/pdf")]
        public async Task<IActionResult> GerarRelatorioVeiculosPdf()
        {
            _logger.LogInformation("[GET] Iniciando geração de relatório em PDF para veículos.");
            try
            {
                // Configuração obrigatória do QuestPDF para projetos gratuitos/pessoais
                QuestPDF.Settings.License = LicenseType.Community;

                // 1. Busca os dados reais do banco
                var veiculos = await _dadosService.ListarVeiculo();

                // 2. Desenha a estrutura do documento
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        // -- Cabeçalho
                        page.Header().Text("Relatório de Veículos - Iveco")
                            .SemiBold().FontSize(20).FontColor(Colors.Green.Darken2);

                        // -- Conteúdo (Tabela)
                        page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                        {
                            // Define 3 colunas iguais
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(); // Coluna VIN
                                columns.RelativeColumn(); // Coluna Modelo
                                columns.RelativeColumn(); // Coluna Data
                            });

                            // Cabeçalho da tabela
                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("VIN").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Modelo").SemiBold();
                                header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Data de Montagem").SemiBold();
                            });

                            // Preenche as linhas da tabela iterando sobre a lista
                            foreach (var v in veiculos)
                            {
                                table.Cell().PaddingVertical(5).Text(v.Vin);
                                table.Cell().PaddingVertical(5).Text(v.Modelo);
                                table.Cell().PaddingVertical(5).Text(v.DataMontagem?.ToString("dd/MM/yyyy HH:mm") ?? "N/A");
                            }
                        });

                        // -- Rodapé
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                });

                // 3. Gera o arquivo em memória e o devolve como download (application/pdf)
                byte[] pdfBytes = document.GeneratePdf();
                _logger.LogInformation("[GET] PDF gerado com sucesso. Enviando para download.");
                return File(pdfBytes, "application/pdf", "Relatorio_Veiculos.pdf");
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Erro ao gerar relatório em PDF");
            }
        }

        /// <summary>
        /// Cria e salva um novo veiculo no banco de dados.
        /// </summary>
        /// <param name="veiculo">O objeto JSON contendo os dados do veiculo a ser criado.</param>
        /// <returns>O veiculo recém-criado junto com a rota para acessá-lo.</returns>
        /// <response code="201">Retorna o veiculo criado e o cabeçalho Location com a URI de acesso.</response>
        /// <response code="400">Se os dados enviados forem nulos ou o nome do veiculo estiver vazio.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar salvar.</response>
        [Tags("Veículos")]
        [HttpPost("veiculos")]
        public async Task<IActionResult> PostVeiculo([FromBody] Veiculo veiculo)
        {
            _logger.LogInformation("[POST] Requisição para criar novo veículo.");
            if (veiculo == null) return BadRequest(new { Erro = "Dados inválidos", Mensagem = "Requisição nula." });
            if (string.IsNullOrWhiteSpace(veiculo.Vin)) return BadRequest(new { Erro = "Campo Obrigatório", Mensagem = "VIN vazio." });

            try
            {
                var todos = await _dadosService.ListarVeiculo();
                if (todos.Any(v => v.Vin.Equals(veiculo.Vin, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning($"[POST] Tentativa de cadastro duplicado rejeitada para o VIN: {veiculo.Vin}");
                    return Conflict(new { Erro = "Duplicado", Mensagem = $"Veículo com VIN '{veiculo.Vin}' já cadastrado!" });
                }

                var criado = await _dadosService.CriarVeiculo(veiculo);
                _logger.LogInformation($"[POST] Veículo criado com sucesso. VIN: {criado.Vin}");
                return Ok(new { mensagem = "Veículo registrado com sucesso!", veiculo = criado });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao criar veículo"); }
        }

        /// <summary>
        /// Atualiza os dados de um veículo existente.
        /// </summary>
        /// <param name="vin">O VIN do veículo a ser atualizado.</param>
        /// <param name="veiculo">Objeto JSON com os dados novos.</param>
        [Tags("Veículos")]
        [HttpPut("veiculos/{vin}")]
        public async Task<IActionResult> PutVeiculo(string vin, [FromBody] Veiculo veiculo)
        {
            _logger.LogInformation($"[PUT] Requisição para atualizar veículo. VIN: {vin}");
            if (veiculo == null)
                return BadRequest(new { Erro = "Dados inválidos", Mensagem = "Requisição nula." });

            if (vin != veiculo.Vin)
            {
                _logger.LogWarning($"[PUT] Inconsistência de VIN: URL ({vin}) difere do Corpo ({veiculo.Vin}).");
                return BadRequest(new { Erro = "Inconsistência", Mensagem = "O VIN da URL diverge do VIN no corpo da requisição." });
            }

            try
            {
                var atualizado = await _dadosService.AtualizarVeiculo(vin, veiculo);

                if (atualizado == null)
                {
                    _logger.LogWarning($"[PUT] Veículo não encontrado para atualização. VIN: {vin}");
                    return NotFound(new { Erro = "Não encontrado", Mensagem = "Veículo não encontrado para atualização." });
                }

                _logger.LogInformation($"[PUT] Veículo atualizado com sucesso. VIN: {vin}");
                return Ok(new { mensagem = "Veículo atualizado com sucesso!", veiculo = atualizado });
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Erro ao atualizar veículo");
            }
        }

        /// <summary>
        /// Exclui um veiculo do banco de dados.
        /// </summary>
        /// <param name="vin">O VIN do veiculo que será excluído.</param>
        /// <returns>Uma mensagem de sucesso confirmando a exclusão.</returns>
        /// <response code="200">Se o veiculo for excluído com sucesso.</response>
        /// <response code="404">Se o veiculo com o ID especificado não for encontrado.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar excluir.</response>
        [Tags("Veículos")]
        [HttpDelete("veiculos/{vin}")]
        public async Task<IActionResult> DeleteVeiculo(string vin)
        {
            _logger.LogInformation($"[DELETE] Requisição para excluir veículo. VIN: {vin}");
            try
            {
                var existente = await _dadosService.ObterVeiculoPorVin(vin);
                if (existente == null)
                {
                    _logger.LogWarning($"[DELETE] Veículo não encontrado para exclusão. VIN: {vin}");
                    return NotFound(new { Erro = "Não encontrado", Mensagem = "Veículo não encontrado." });
                }

                await _dadosService.ExcluirVeiculo(vin);
                _logger.LogInformation($"[DELETE] Veículo excluído com sucesso. VIN: {vin}");
                return Ok(new { mensagem = "Veículo Deletado com Sucesso" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao deletar veículo"); }
        }

        /// <summary>
        /// Descodifica um VIN e valida se pertence obrigatoriamente à marca IVECO.
        /// </summary>
        [Tags("Veículos")]
        [HttpGet("veiculos/validar-vin/{vin}")]
        public async Task<IActionResult> ValidarVinIveco(string vin)
        {
            _logger.LogInformation($"[GET] Validando VIN na base NHTSA. VIN: {vin}");
            try
            {
                if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
                {
                    _logger.LogWarning($"[GET] Validação rejeitada: VIN com tamanho inválido ({vin?.Length ?? 0} caracteres).");
                    return BadRequest(new { Erro = "VIN Inválido", Mensagem = "O VIN deve conter exatamente 17 caracteres." });
                }

                var veiculoIveco = await _dadosService.BuscarEValidarVinIvecoAsync(vin);

                if (veiculoIveco == null)
                {
                    _logger.LogWarning($"[GET] Dados não encontrados na NHTSA para o VIN: {vin}");
                    return NotFound(new { Erro = "Não encontrado", Mensagem = "Não foi possível descodificar os dados deste VIN." });
                }

                _logger.LogInformation($"[GET] Veículo IVECO validado com sucesso. VIN: {vin}");
                return Ok(new { mensagem = "Veículo IVECO validado com sucesso!", veiculo = veiculoIveco });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Apenas veículos IVECO são permitidos"))
                {
                    _logger.LogWarning($"[ALERTA] Tentativa de validação com marca não autorizada. Erro: {ex.Message}");
                    return StatusCode(403, new { Erro = "Marca não autorizada", Mensagem = ex.Message });
                }

                return TratarErro(ex, "Erro ao validar VIN na NHTSA");
            }
        }

        /// <summary>
        /// Retorna a lista completa de fornecedores cadastrados no banco de dados.
        /// </summary>
        /// <returns>Uma lista contendo todos os fornecedores.</returns>
        /// <response code="200">Retorna a lista de fornecedores com sucesso.</response>
        /// <response code="400">Se houver algum problema com os parâmetros de consulta.</response>
        /// <response code="404">Se a coleção de fornecedores não for encontrada.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor (ex: falha no Firebase).</response>
        [Tags("Fornecedores")]
        [HttpGet("fornecedores")]
        public async Task<IActionResult> GetFornecedores()
        {
            _logger.LogInformation("[GET] Requisição para listar fornecedores.");
            try
            {
                var fornecedores = await _dadosService.ListarFornecedor();
                _logger.LogInformation($"[GET] Listados {fornecedores.Count} fornecedores.");
                return Ok(fornecedores);
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao listar fornecedores"); }
        }

        /// <summary>
        /// Busca o nome e endereço de uma empresa na Receita Federal via BrasilAPI.
        /// </summary>
        /// <param name="cnpj">O CNPJ da empresa a ser buscada (com ou sem pontuação).</param>
        /// <returns>Os dados do fornecedor formatados e prontos para cadastro.</returns>
        /// <response code="200">Retorna os dados da empresa localizados com sucesso.</response>
        /// <response code="400">Se o CNPJ fornecido for nulo ou vazio.</response>
        /// <response code="404">Se o CNPJ não for encontrado na base da Receita Federal.</response>
        /// <response code="500">Se ocorrer um erro interno na comunicação com a BrasilAPI.</response>
        [Tags("Fornecedores")]
        [HttpGet("fornecedores/buscar-cnpj/{cnpj}")]
        public async Task<IActionResult> GetFornecedorCnpj(string cnpj)
        {
            _logger.LogInformation($"[GET] Buscando dados de fornecedor pelo CNPJ: {cnpj}");
            try
            {
                if (string.IsNullOrWhiteSpace(cnpj))
                {
                    _logger.LogWarning("[GET] Busca falhou: CNPJ não fornecido.");
                    return BadRequest(new { Erro = "CNPJ obrigatório", Mensagem = "Digite o CNPJ da empresa." });
                }

                var proveedores = await _dadosService.BuscarFornecedorPorCnpjAsync(cnpj);

                if (proveedores == null)
                {
                    _logger.LogWarning($"[GET] CNPJ não encontrado na BrasilAPI: {cnpj}");
                    return NotFound(new { Erro = "Não encontrado", Mensagem = "CNPJ não encontrado na base da Receita Federal." });
                }

                _logger.LogInformation($"[GET] Fornecedor localizado: {proveedores.Nome}");
                return Ok(new { mensagem = "Fornecedor localizado com sucesso!", fornecedor = proveedores });
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Erro ao buscar CNPJ na BrasilAPI");
            }
        }

        /// <summary>
        /// Cria e salva um novo fornecedor no banco de dados.
        /// </summary>
        /// <param name="fornecedor">O objeto JSON contendo os dados do fornecedor a ser criado.</param>
        /// <returns>O fornecedor recém-criado junto com a rota para acessá-lo.</returns>
        /// <response code="201">Retorna o fornecedor criado e o cabeçalho Location com a URI de acesso.</response>
        /// <response code="400">Se os dados enviados forem nulos ou o nome do fornecedor estiver vazio.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar salvar.</response>
        [Tags("Fornecedores")]
        [HttpPost("fornecedores")]
        public async Task<IActionResult> PostFornecedor([FromBody] Fornecedor fornecedor)
        {
            _logger.LogInformation("[POST] Requisição para criar novo fornecedor.");
            if (fornecedor == null) return BadRequest("Dados do fornecedor não podem ser nulos.");
            try
            {
                var criado = await _dadosService.CriarFornecedor(fornecedor);
                _logger.LogInformation($"[POST] Fornecedor criado com sucesso. ID/CNPJ: {criado.Cnpj}");
                return Ok(new { mensagem = "Fornecedor registrado com sucesso!", fornecedor = criado });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao criar fornecedor"); }
        }

        /// <summary>
        /// Exclui um fornecedores do banco de dados.
        /// </summary>
        /// <param name="id">O ID do fornencedores que será excluído.</param>
        /// <returns>Uma mensagem de sucesso confirmando a exclusão.</returns>
        /// <response code="200">Se o fornecedores for excluído com sucesso.</response>
        /// <response code="404">Se o fornecedores com o ID especificado não for encontrado.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar excluir.</response>
        [Tags("Fornecedores")]
        [HttpDelete("fornecedores/{id}")]
        public async Task<IActionResult> DeleteFornecedor(string id)
        {
            _logger.LogInformation($"[DELETE] Requisição para excluir fornecedor. ID: {id}");
            try
            {
                await _dadosService.ExcluirFornecedor(id);
                _logger.LogInformation($"[DELETE] Fornecedor excluído com sucesso. ID: {id}");
                return Ok(new { mensagem = "Fornecedor Deletado com Sucesso" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao deletar fornecedor"); }
        }

        /// <summary>
        /// Retorna a lista completa de lotes cadastrados no banco de dados.
        /// </summary>
        /// <returns>Uma lista contendo todos os lotes.</returns>
        /// <response code="200">Retorna a lista de lotes com sucesso.</response>
        /// <response code="400">Se houver algum problema com os parâmetros de consulta.</response>
        /// <response code="404">Se a coleção de lotes não for encontrada.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor (ex: falha no Firebase).</response>
        [Tags("Lotes e Componentes")]
        [HttpGet("lotes")]
        public async Task<IActionResult> GetLotes()
        {
            _logger.LogInformation("[GET] Requisição para listar lotes.");
            try
            {
                var lotes = await _dadosService.ListarLoteMateriaPrima();
                _logger.LogInformation($"[GET] Listados {lotes.Count} lotes.");
                return Ok(lotes);
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao listar lotes"); }
        }

        /// <summary>
        /// Cria e salva um novo lote no banco de dados.
        /// </summary>
        /// <param name="lote">O objeto JSON contendo os dados do lote a ser criado.</param>
        /// <returns>O lote recém-criado junto com a rota para acessá-lo.</returns>
        /// <response code="201">Retorna o lote criado e o cabeçalho Location com a URI de acesso.</response>
        /// <response code="400">Se os dados enviados forem nulos ou o nome do lote estiver vazio.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar salvar.</response>
        [Tags("Lotes e Componentes")]
        [HttpPost("lotes")]
        public async Task<IActionResult> PostLote([FromBody] LoteMateriaPrima lote)
        {
            _logger.LogInformation("[POST] Requisição para criar novo lote.");
            if (lote == null) return BadRequest("Dados do lote não podem ser nulos.");
            try
            {
                var criado = await _dadosService.CriarLoteMateriaPrima(lote);
                _logger.LogInformation($"[POST] Lote registrado com sucesso. ID: {criado.Id}");
                return Ok(new { mensagem = "Lote registrado com sucesso!", lote = criado });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao criar lote"); }
        }

        /// <summary>
        /// Exclui um lote do banco de dados.
        /// </summary>
        /// <param name="id">O ID do lote que será excluído.</param>
        /// <returns>Uma mensagem de sucesso confirmando a exclusão.</returns>
        /// <response code="200">Se o lote for excluído com sucesso.</response>
        /// <response code="404">Se o lote com o ID especificado não for encontrado.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar excluir.</response>
        [Tags("Lotes e Componentes")]
        [HttpDelete("lotes/{id}")]
        public async Task<IActionResult> DeleteLote(string id)
        {
            _logger.LogInformation($"[DELETE] Requisição para excluir lote. ID: {id}");
            try
            {
                await _dadosService.ExcluirLoteMateriaPrima(id);
                _logger.LogInformation($"[DELETE] Lote excluído com sucesso. ID: {id}");
                return Ok(new { mensagem = "Lote Deletado com Sucesso" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao deletar lote"); }
        }

        /// <summary>
        /// Retorna a lista completa de componentes cadastrados no banco de dados.
        /// </summary>
        /// <returns>Uma lista contendo todos os componentes.</returns>
        /// <response code="200">Retorna a lista de componentes com sucesso.</response>
        /// <response code="400">Se houver algum problema com os parâmetros de consulta.</response>
        /// <response code="404">Se a coleção de componentes não for encontrada.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor (ex: falha no Firebase).</response>
        [Tags("Lotes e Componentes")]
        [HttpGet("componentes")]
        public async Task<IActionResult> GetComponentes()
        {
            _logger.LogInformation("[GET] Requisição para listar componentes.");
            try
            {
                var componentes = await _dadosService.ListarVeiculoComponente();
                _logger.LogInformation($"[GET] Listados {componentes.Count} componentes.");
                return Ok(componentes);
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao listar componentes"); }
        }

        /// <summary>
        /// Cria e salva um novo componente no banco de dados.
        /// </summary>
        /// <param name="componente">O objeto JSON contendo os dados do componente a ser criado.</param>
        /// <returns>O componente recém-criado junto com a rota para acessá-lo.</returns>
        /// <response code="201">Retorna o componente criado e o cabeçalho Location com a URI de acesso.</response>
        /// <response code="400">Se os dados enviados forem nulos ou o nome do componente estiver vazio.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar salvar.</response>
        [Tags("Lotes e Componentes")]
        [HttpPost("componentes")]
        public async Task<IActionResult> PostComponente([FromBody] VeiculoComponente componente)
        {
            _logger.LogInformation("[POST] Requisição para criar novo componente.");
            if (componente == null) return BadRequest("Dados do componente não podem ser nulos.");
            try
            {
                var criado = await _dadosService.CriarVeiculoComponente(componente);
                _logger.LogInformation($"[POST] Componente registrado com sucesso. ID: {criado.Id}");
                return Ok(new { mensagem = "Componente registrado com sucesso!", componente = criado });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao criar componente"); }
        }

        /// <summary>
        /// Exclui um componente do banco de dados.
        /// </summary>
        /// <param name="id">O ID do componente que será excluído.</param>
        /// <returns>Uma mensagem de sucesso confirmando a exclusão.</returns>
        /// <response code="200">Se o componente for excluído com sucesso.</response>
        /// <response code="404">Se o componente com o ID especificado não for encontrado.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar excluir.</response>
        [Tags("Lotes e Componentes")]
        [HttpDelete("componentes/{id}")]
        public async Task<IActionResult> DeleteComponente(string id)
        {
            _logger.LogInformation($"[DELETE] Requisição para excluir componente. ID: {id}");
            try
            {
                await _dadosService.ExcluirVeiculoComponente(id);
                _logger.LogInformation($"[DELETE] Componente excluído com sucesso. ID: {id}");
                return Ok(new { mensagem = "Componente Deletado com Sucesso" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao deletar componente"); }
        }

        /// <summary>
        /// Cadastra um novo usuário no sistema (Firebase).
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/auth/cadastrar
        ///     {
        ///        "nome": "João Admin",
        ///        "email": "joao@iveco.com",
        ///        "senha": "senha_segura",
        ///        "perfil": "Admin"
        ///     }
        ///     
        /// Obs: Se o campo "perfil" não for enviado, o sistema atribuirá "Usuario" por padrão.
        /// </remarks>
        /// <param name="usuario">O objeto JSON contendo os dados do usuário a ser cadastrado.</param>
        /// <returns>O usuário recém-criado com a senha omitida por segurança.</returns>
        /// <response code="200">Retorna o usuário cadastrado com sucesso.</response>
        /// <response code="400">Se o e-mail ou a senha estiverem nulos, vazios ou inválidos.</response>
        /// <response code="500">Se ocorrer um erro no banco (ex: E-mail já existente no Firebase).</response>
        [Tags("Autenticação")]
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] ApiIveco.Models.Usuario usuario)
        {
            _logger.LogInformation($"[POST] Tentativa de cadastro de novo usuário. E-mail: {usuario?.Email}");
            if (string.IsNullOrWhiteSpace(usuario.Email) || string.IsNullOrWhiteSpace(usuario.Senha))
            {
                _logger.LogWarning("[POST] Cadastro falhou: E-mail ou Senha vazios.");
                return BadRequest(new { Erro = "Dados Inválidos", Mensagem = "E-mail e senha são obrigatórios." });
            }

            try
            {
                // Se não enviar perfil, vira "Usuario" padrão. Se enviar "Admin", vira Admin.
                if (string.IsNullOrWhiteSpace(usuario.Acesso))
                    usuario.Acesso = "Usuario";

                var criado = await _dadosService.CadastrarUsuario(usuario);

                // Oculta a senha antes de devolver a resposta por segurança
                criado.Senha = "";
                _logger.LogInformation($"[POST] Usuário cadastrado com sucesso. ID: {criado.Id}");
                return Ok(new { mensagem = "Usuário cadastrado com sucesso!", usuario = criado });
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Falha no Cadastro");
            }
        }

        /// <summary>
        /// Autentica um usuário existente no sistema utilizando E-mail e Senha.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///        "email": "joao@iveco.com",
        ///        "senha": "senha_segura"
        ///     }
        /// </remarks>
        /// <param name="credenciais">Objeto JSON contendo as credenciais de acesso (Email e Senha).</param>
        /// <returns>Os dados do usuário autenticado, incluindo o seu Perfil (Admin/Usuario).</returns>
        /// <response code="200">Login efetuado com sucesso. Retorna os dados da sessão.</response>
        /// <response code="400">Se os dados enviados forem inválidos (falta de e-mail ou senha).</response>
        /// <response code="401">Se as credenciais estiverem incorretas (E-mail não encontrado ou senha errada).</response>
        /// <response code="500">Se ocorrer um erro interno no servidor de autenticação.</response>
        [Tags("Autenticação")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest credenciais)
        {
            _logger.LogInformation($"[POST] Tentativa de Login efetuada para o E-mail: {credenciais?.Email}");
            if (string.IsNullOrWhiteSpace(credenciais.Email) || string.IsNullOrWhiteSpace(credenciais.Senha))
            {
                _logger.LogWarning("[POST] Login bloqueado: E-mail ou senha não informados.");
                return BadRequest(new
                {
                    Erro = "Dados Inválidos",
                    Mensagem = "Informe e-mail e senha."
                });
            }

            try
            {
                var usuario = await _dadosService.FazerLogin(credenciais.Email, credenciais.Senha);

                if (usuario == null)
                {
                    _logger.LogWarning($"[POST] Falha no Login: Credenciais inválidas para o E-mail: {credenciais.Email}");
                    return Unauthorized(new
                    {
                        Erro = "Acesso Negado",
                        Mensagem = "E-mail ou senha incorretos."
                    });
                }

                usuario.Senha = "";
                _logger.LogInformation($"[POST] Login bem-sucedido para: {usuario.Nome} (Acesso: {usuario.Acesso})");
                return Ok(new { mensagem = "Login efetuado com sucesso!", usuario });
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Erro no Login");
            }
        }

        // ==========================================
        // MÉTODO AUXILIAR DE TRATAMENTO DE ERROS
        // ==========================================
        private IActionResult TratarErro(Exception ex, string contexto)
        {
            // O ILogger agora regista o erro com toda a StackTrace e a mensagem em vermelho no terminal
            _logger.LogError(ex, $"[ERRO CRÍTICO] {contexto} -> {ex.Message}");

            if (ex is ArgumentNullException || ex is ArgumentException)
                return StatusCode(StatusCodes.Status400BadRequest, new { Erro = "Dados inválidos", Mensagem = ex.Message });

            if (ex is KeyNotFoundException)
                return StatusCode(StatusCodes.Status404NotFound, new { Erro = "Não encontrado", Mensagem = ex.Message });

            return StatusCode(StatusCodes.Status500InternalServerError, new { Erro = "Falha Interna", Mensagem = ex.Message });
        }

        // Classe auxiliar só para o login — coloque fora da classe DadosController
        public class LoginRequest
        {
            public string Email { get; set; }
            public string Senha { get; set; }
        }
    }
}