using ApiIveco.DTO;
using ApiIveco.Models;
using ApiIveco.Service;
using ApiIveco.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApiIveco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DadosController : ControllerBase
    {
        private readonly DadosService _dadosService;
        private readonly ILogger<DadosController> _logger;
        private readonly IEmailValidationService _emailValidationService;

        public DadosController(DadosService dadosService, ILogger<DadosController> logger, IEmailValidationService emailValidationService)
        {
            _dadosService = dadosService;
            _logger = logger;
            _emailValidationService = emailValidationService;
        }

        // =====================================================================
        // VEÍCULOS
        // =====================================================================

        [Tags("Veículos")]
        [HttpGet("veiculos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVeiculos()
        {
            _logger.LogInformation("[GET] Listando todos os veículos.");
            var veiculos = await _dadosService.ListarVeiculo();
            return Ok(veiculos);
        }

        [Tags("Veículos")]
        [HttpGet("veiculos/{vin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetVeiculoByVin(string vin)
        {
            _logger.LogInformation("[GET] Buscando veículo. VIN: {vin}", vin);

            if (string.IsNullOrWhiteSpace(vin))
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var veiculo = await _dadosService.ObterVeiculoPorVin(vin);
            if (veiculo == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            return Ok(new { mensagem = "Veículo encontrado", veiculo });
        }

        [Tags("Veículos")]
        [HttpPost("veiculos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostVeiculo([FromBody] Veiculo veiculo)
        {
            _logger.LogInformation("[POST] Criando novo veículo.");

            if (veiculo == null || string.IsNullOrWhiteSpace(veiculo.Vin))
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var todos = await _dadosService.ListarVeiculo();
            if (todos.Any(v => v.Vin.Equals(veiculo.Vin, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("[POST] VIN duplicado rejeitado: {vin}", veiculo.Vin);
                return Conflict(new { Mensagem = $"Veículo com VIN '{veiculo.Vin}' já cadastrado." });
            }

            var criado = await _dadosService.CriarVeiculo(veiculo);

            try
            {
                await _dadosService.GerarComponentesParaVeiculoAsync(criado.Vin);
            }
            catch
            {
                _logger.LogWarning("Veículo criado, mas falhou ao vincular peças automaticamente.");
            }

            return Ok(new { mensagem = "Veículo registrado e peças vinculadas com sucesso!", veiculo = criado });
        }

        [Tags("Veículos")]
        [HttpPut("veiculos/{vin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutVeiculo(string vin, [FromBody] Veiculo veiculo)
        {
            _logger.LogInformation("[PUT] Atualizando veículo. VIN: {vin}", vin);

            if (veiculo == null || vin != veiculo.Vin)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var atualizado = await _dadosService.AtualizarVeiculo(vin, veiculo);
            if (atualizado == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            return Ok(new { mensagem = "Veículo atualizado com sucesso!", veiculo = atualizado });
        }

        [Tags("Veículos")]
        [HttpDelete("veiculos/{vin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteVeiculo(string vin)
        {
            _logger.LogInformation("[DELETE] Excluindo veículo. VIN: {vin}", vin);

            var existente = await _dadosService.ObterVeiculoPorVin(vin);
            if (existente == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            await _dadosService.ExcluirVeiculo(vin);
            return Ok(new { mensagem = "Veículo deletado com sucesso." });
        }

        [Tags("Veículos")]
        [HttpGet("veiculos/validar-vin/{vin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidarVinIveco(string vin)
        {
            _logger.LogInformation("[GET] Validando VIN IVECO: {vin}", vin);

            if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos. O VIN deve ter 17 caracteres." });

            try
            {
                var veiculoIveco = await _dadosService.BuscarEValidarVinIvecoAsync(vin);
                if (veiculoIveco == null)
                    return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado na NHTSA." });

                return Ok(new { mensagem = "Veículo IVECO validado com sucesso!", veiculo = veiculoIveco });
            }
            catch
            {
                _logger.LogWarning("[GET] Rejeição de VIN: {vin}", vin);
                return BadRequest(new { Mensagem = "O VIN informado não pertence a um veículo IVECO válido." });
            }
        }

        [Tags("Veículos")]
        [HttpGet("relatorios/veiculos/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GerarRelatorioVeiculosPdf()
        {
            _logger.LogInformation("[GET] Gerando relatório PDF de veículos.");

            QuestPDF.Settings.License = LicenseType.Community;

            var veiculos = await _dadosService.ListarVeiculo();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text("Relatório de Veículos - Iveco")
                        .SemiBold().FontSize(20).FontColor(Colors.Green.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("VIN").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Modelo").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(5).Text("Data de Montagem").SemiBold();
                        });

                        foreach (var v in veiculos)
                        {
                            table.Cell().PaddingVertical(5).Text(v.Vin);
                            table.Cell().PaddingVertical(5).Text(v.Modelo);
                            table.Cell().PaddingVertical(5).Text(v.DataMontagem?.ToString("dd/MM/yyyy HH:mm") ?? "N/A");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "Relatorio_Veiculos.pdf");
        }

        [HttpGet]
        public IActionResult Get() => Ok(new { status = "OK", timestamp = DateTime.UtcNow });

        // =====================================================================
        // FORNECEDORES
        // =====================================================================

        [Tags("Fornecedores")]
        [HttpGet("fornecedores")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFornecedores()
        {
            _logger.LogInformation("[GET] Listando fornecedores.");
            var fornecedores = await _dadosService.ListarFornecedor();
            return Ok(fornecedores);
        }

        [Tags("Fornecedores")]
        [HttpGet("fornecedores/buscar-cnpj/{cnpj}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFornecedorCnpj(string cnpj)
        {
            _logger.LogInformation("[GET] Buscando CNPJ: {cnpj}", cnpj);

            if (string.IsNullOrWhiteSpace(cnpj))
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var fornecedor = await _dadosService.BuscarFornecedorPorCnpjAsync(cnpj);
            if (fornecedor == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            return Ok(new { mensagem = "Fornecedor localizado com sucesso!", fornecedor });
        }

        [Tags("Fornecedores")]
        [HttpPost("fornecedores")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostFornecedor([FromBody] Fornecedor fornecedor)
        {
            _logger.LogInformation("[POST] Criando fornecedor.");

            if (fornecedor == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var criado = await _dadosService.CriarFornecedor(fornecedor);
            return Ok(new { mensagem = "Fornecedor registrado com sucesso!", fornecedor = criado });
        }

        [Tags("Fornecedores")]
        [HttpDelete("fornecedores/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteFornecedor(string id)
        {
            _logger.LogInformation("[DELETE] Excluindo fornecedor. ID: {id}", id);
            await _dadosService.ExcluirFornecedor(id);
            return Ok(new { mensagem = "Fornecedor deletado com sucesso." });
        }

        // =====================================================================
        // LOTES
        // =====================================================================

        [Tags("Lotes e Componentes")]
        [HttpGet("lotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLotes()
        {
            _logger.LogInformation("[GET] Listando lotes.");
            var lotes = await _dadosService.ListarLoteMateriaPrima();
            return Ok(lotes);
        }

        [Tags("Lotes e Componentes")]
        [HttpPost("lotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostLote([FromBody] LoteMateriaPrima lote)
        {
            _logger.LogInformation("[POST] Criando lote.");

            if (lote == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var criado = await _dadosService.CriarLoteMateriaPrima(lote);
            return Ok(new { mensagem = "Lote registrado com sucesso!", lote = criado });
        }

        [Tags("Lotes e Componentes")]
        [HttpDelete("lotes/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteLote(string id)
        {
            _logger.LogInformation("[DELETE] Excluindo lote. ID: {id}", id);
            await _dadosService.ExcluirLoteMateriaPrima(id);
            return Ok(new { mensagem = "Lote deletado com sucesso." });
        }

        // =====================================================================
        // COMPONENTES
        // =====================================================================

        [Tags("Lotes e Componentes")]
        [HttpGet("componentes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetComponentes()
        {
            _logger.LogInformation("[GET] Listando componentes.");
            var componentes = await _dadosService.ListarVeiculoComponente();
            return Ok(componentes);
        }

        // =====================================================================
        // MÉTODO CORRIGIDO: PostComponente
        // =====================================================================
        [Tags("Lotes e Componentes")]
        [HttpPost("componentes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostComponente([FromBody] VeiculoComponente componente)
        {
            _logger.LogInformation("[POST] Criando componente.");

            if (componente == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            try
            {
                var criado = await _dadosService.CriarVeiculoComponente(componente);
                return Ok(new { mensagem = "Componente registrado com sucesso!", componente = criado });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar componente");
                return StatusCode(500, new { Mensagem = "Erro interno ao registrar componente: " + ex.Message });
            }
        }

        [Tags("Lotes e Componentes")]
        [HttpDelete("componentes/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteComponente(string id)
        {
            _logger.LogInformation("[DELETE] Excluindo componente. ID: {id}", id);
            await _dadosService.ExcluirVeiculoComponente(id);
            return Ok(new { mensagem = "Componente deletado com sucesso." });
        }

        // =====================================================================
        // AUTENTICAÇÃO
        // =====================================================================

        [Tags("Autenticação")]
        [HttpPost("cadastrar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cadastrar([FromBody] ApiIveco.Models.Usuario usuario)
        {
            _logger.LogInformation("[POST] Cadastro solicitado. E-mail: {email}", usuario?.Email);

            if (string.IsNullOrWhiteSpace(usuario?.Email) || string.IsNullOrWhiteSpace(usuario?.Senha))
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            if (string.IsNullOrWhiteSpace(usuario.Acesso))
                usuario.Acesso = "Usuario";

            var criado = await _dadosService.CadastrarUsuario(usuario);
            criado.Senha = "";
            return Ok(new { mensagem = "Usuário cadastrado com sucesso!", usuario = criado });
        }

        [Tags("Autenticação")]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto credenciais)
        {
            _logger.LogInformation("[POST] Tentativa de login. E-mail: {email}", credenciais?.Email);

            if (string.IsNullOrWhiteSpace(credenciais?.Email) || string.IsNullOrWhiteSpace(credenciais?.Senha))
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var usuario = await _dadosService.FazerLogin(credenciais.Email, credenciais.Senha);
            if (usuario == null)
            {
                _logger.LogWarning("[POST] Login falhou. E-mail: {email}", credenciais.Email);
                return Unauthorized(new { Mensagem = "Credenciais incorretas. Verifique o e-mail e a senha." });
            }

            usuario.Senha = "";
            return Ok(new { mensagem = "Login efetuado com sucesso!", usuario });
        }

        [Tags("Autenticação")]
        [HttpGet("validar-email")]
        public async Task<IActionResult> ValidarEmail([FromQuery] string email)
        {
            _logger.LogInformation("[GET] Validando e-mail: {email}", email);
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { Mensagem = "O e-mail é obrigatório." });

            var (isValid, message) = await _emailValidationService.ValidateEmailAsync(email);
            _logger.LogInformation("[GET] Resultado para {email}: valido={isValid}", email);

            return Ok(new { valido = isValid, mensagem = message });
        }

        // =====================================================================
        // RECUPERAÇÃO DE SENHA (SIMULADO)
        // =====================================================================
        [HttpPost("recuperar-senha")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecuperarSenha([FromBody] EmailDto dto)
        {
            _logger.LogInformation($"Solicitação de recuperação para: {dto.Email}");
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { Mensagem = "E-mail é obrigatório." });

            // Aqui você implementaria o envio de e-mail real.
            // Por enquanto, apenas logamos e retornamos sucesso.
            return Ok(new { mensagem = "Instruções de recuperação enviadas (simulado)." });
        }

        // =====================================================================
        // DASHBOARD / ESG
        // =====================================================================

        [Tags("Dashboard")]
        [HttpGet("pegada-media")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPegadaMedia()
        {
            _logger.LogInformation("[GET] Calculando pegada média.");
            var media = await _dadosService.CalcularPegadaMediaAsync();
            return Ok(new { pegadaMedia = media });
        }

        [Tags("Dashboard")]
        [HttpGet("grafico-emissoes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDadosGrafico()
        {
            var dados = await _dadosService.ObterDadosGraficoAsync();
            return Ok(dados);
        }

        [Tags("Dashboard")]
        [HttpGet("analises-esg")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDadosAnalisesESG()
        {
            var dados = await _dadosService.ObterDadosAnalisesESGAsync();
            return Ok(dados);
        }

        [Tags("Dashboard")]
        [HttpGet("total-emissoes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalEmissoes()
        {
            _logger.LogInformation("[GET] Calculando total de emissões.");
            try
            {
                var total = await _dadosService.CalcularTotalEmissoesAsync();
                return Ok(new { totalEmissoes = total });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao calcular total de emissões: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao calcular total de emissões." });
            }
        }

        [Tags("Dashboard")]
        [HttpGet("preco-carbono")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPrecoCarbono()
        {
            var preco = await _dadosService.ObterPrecoCarbonoAsync();
            return Ok(new { preco = preco });
        }
    }

    // DTO para recuperação de senha
    public class EmailDto
    {
        public string Email { get; set; }
    }
}