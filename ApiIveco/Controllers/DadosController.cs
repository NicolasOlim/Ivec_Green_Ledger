using ApiIveco.Models;
using ApiIveco.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiIveco.Controllers
{
    /// <summary>
    /// Gerencia veículos, fornecedores, lotes, componentes e autenticação.
    /// </summary>
    /// <remarks>Operações baseadas no Firebase Firestore. Logs registram ações críticas.</remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class DadosController : ControllerBase
    {
        private readonly DadosService _dadosService;
        private readonly ILogger<DadosController> _logger;

        public DadosController(DadosService dadosService, ILogger<DadosController> logger)
        {
            _dadosService = dadosService;
            _logger = logger;
        }


        /// VEÍCULOS


        /// <summary>Lista todos os veículos.</summary>
        /// <remarks>Recupera a coleção elástica NoSQL de veículos armazenados diretamente no repositório Firestore.</remarks>
        /// <response code="200">Lista de veículos (pode ser vazia).</response>
        [Tags("Veículos")]
        [HttpGet("veiculos")]
        public async Task<IActionResult> GetVeiculos()
        {
            _logger.LogInformation("[GET] Listando todos os veículos.");
            var veiculos = await _dadosService.ListarVeiculo();
            return Ok(veiculos);
        }

        /// <summary>Busca um veículo pelo VIN.</summary>
        /// <remarks>Efetua a busca linear por correspondência exata baseando-se no identificador único do documento (VIN).</remarks>
        /// <param name="vin">VIN do veículo (17 caracteres).</param>
        /// <response code="200">Veículo encontrado.</response>
        /// <response code="400">VIN inválido.</response>
        /// <response code="404">Não encontrado.</response>
        [Tags("Veículos")]
        [HttpGet("veiculos/{vin}")]
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

        /// <summary>Cadastra um novo veículo.</summary>
        /// <remarks>O VIN deve ser único. Em caso de duplicidade, retorna 409 Conflict antes de processar a inserção na nuvem.</remarks>
        /// <param name="veiculo">Dados do veículo.</param>
        /// <response code="200">Veículo criado.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="409">VIN já cadastrado.</response>
        [Tags("Veículos")]
        [HttpPost("veiculos")]
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

            /// Cria o veículo
            var criado = await _dadosService.CriarVeiculo(veiculo);

            /// NOVA IMPLEMENTAÇÃO: Gera e vincula automaticamente os componentes associados a este VIN
            try
            {
                await _dadosService.GerarComponentesParaVeiculoAsync(criado.Vin);
            }
            catch (Exception ex)
            {
                _logger.LogError("Veículo criado, mas falhou ao vincular peças: {erro}", ex.Message);
                /// Pode optar por devolver um aviso sem quebrar a criação do veículo
            }

            return Ok(new { mensagem = "Veículo registado e peças vinculadas com sucesso!", veiculo = criado });
        }

        /// <summary>Atualiza um veículo existente.</summary>
        /// <remarks>Utiliza o método SetAsync com a diretiva MergeAll do Firestore para mesclar os novos dados com as propriedades existentes sem corromper o histórico do documento.</remarks>
        /// <param name="vin">VIN do veículo a ser updated.</param>
        /// <param name="veiculo">Novos dados.</param>
        /// <response code="200">Atualizado com sucesso.</response>
        /// <response code="400">VIN da URL não confere com o corpo.</response>
        /// <response code="404">Veículo não encontrado.</response>
        [Tags("Veículos")]
        [HttpPut("veiculos/{vin}")]
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

        /// <summary>Remove um veículo do banco de dados.</summary>
        /// <remarks>Realiza a deleção física do documento mapeado pelo VIN na coleção do Firebase Firestore.</remarks>
        /// <param name="vin">VIN do veículo.</param>
        /// <response code="200">Excluído com sucesso.</response>
        /// <response code="404">Veículo não encontrado.</response>
        [Tags("Veículos")]
        [HttpDelete("veiculos/{vin}")]
        public async Task<IActionResult> DeleteVeiculo(string vin)
        {
            _logger.LogInformation("[DELETE] Excluindo veículo. VIN: {vin}", vin);

            var existente = await _dadosService.ObterVeiculoPorVin(vin);
            if (existente == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            await _dadosService.ExcluirVeiculo(vin);
            return Ok(new { mensagem = "Veículo deletado com sucesso." });
        }

        /// <summary>Valida se o VIN pertence à marca IVECO.</summary>
        /// <remarks>Dispara uma requisição HTTP assíncrona para a API governamental da NHTSA, processa o JSON e valida de forma estrita se os metadados de fabricação contêm a marca IVECO.</remarks>
        /// <param name="vin">VIN de 17 caracteres.</param>
        /// <response code="200">VIN válido para IVECO.</response>
        /// <response code="400">VIN com tamanho incorreto.</response>
        /// <response code="404">Não é IVECO ou não encontrado.</response>
        [Tags("Veículos")]
        [HttpGet("veiculos/validar-vin/{vin}")]
        public async Task<IActionResult> ValidarVinIveco(string vin)
        {
            _logger.LogInformation("[GET] Validando VIN IVECO: {vin}", vin);

            if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos. O VIN deve ter 17 caracteres." });

            try
            {
                /// Se não for IVECO, o service dispara uma Exception
                var veiculoIveco = await _dadosService.BuscarEValidarVinIvecoAsync(vin);

                if (veiculoIveco == null)
                    return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado na NHTSA." });

                return Ok(new { mensagem = "Veículo IVECO validado com sucesso!", veiculo = veiculoIveco });
            }
            catch (Exception ex)
            {
                /// Captura a rejeição da marca e devolve para o WPF de forma controlada
                _logger.LogWarning("[GET] Rejeição de VIN: {mensagem}", ex.Message);
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        /// <summary>Gera um relatório PDF com todos os veículos.</summary>
        /// <remarks>Usa a biblioteca QuestPDF sob licença comunitária. Compila os dados dinamicamente estruturando uma tabela A4 que é convertida e retornada em fluxo de bytes binários.</remarks>
        /// <response code="200">PDF gerado com sucesso.</response>
        [Tags("Veículos")]
        [HttpGet("relatorios/veiculos/pdf")]
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


        /// FORNECEDORES


        /// <summary>Lista todos os fornecedores.</summary>
        /// <remarks>Mapeia de forma assíncrona todos os documentos da coleção "fornecedores" injetando os IDs internos do Firebase de volta nas propriedades dos modelos.</remarks>
        [Tags("Fornecedores")]
        [HttpGet("fornecedores")]
        public async Task<IActionResult> GetFornecedores()
        {
            _logger.LogInformation("[GET] Listando fornecedores.");
            var fornecedores = await _dadosService.ListarFornecedor();
            return Ok(fornecedores);
        }

        /// <summary>Busca dados de um CNPJ na Receita Federal via BrasilAPI.</summary>
        /// <remarks>Encapsula regras industriais com User-Agent blindado para consultar informações de cadastro nacional e sanitizar strings de endereçamento de sede corporativa.</remarks>
        /// <param name="cnpj">CNPJ da empresa.</param>
        /// <response code="200">Dados encontrados.</response>
        /// <response code="400">CNPJ vazio.</response>
        /// <response code="404">CNPJ não encontrado.</response>
        [Tags("Fornecedores")]
        [HttpGet("fornecedores/buscar-cnpj/{cnpj}")]
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

        /// <summary>Cadastra um novo fornecedor.</summary>
        /// <remarks>Aciona uma transação atômica síncrona no Firestore para computar e incrementar o ID sequencial na coleção de contadores do banco NoSQL.</remarks>
        /// <param name="fornecedor">Dados do fornecedor.</param>
        [Tags("Fornecedores")]
        [HttpPost("fornecedores")]
        public async Task<IActionResult> PostFornecedor([FromBody] Fornecedor fornecedor)
        {
            _logger.LogInformation("[POST] Criando fornecedor.");

            if (fornecedor == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var criado = await _dadosService.CriarFornecedor(fornecedor);
            return Ok(new { mensagem = "Fornecedor registrado com sucesso!", fornecedor = criado });
        }

        /// <summary>Exclui um fornecedor pelo ID do Firebase.</summary>
        /// <remarks>Exclui fisicamente o nó do documento de fornecedor com base em sua chave primária textual indexada na nuvem.</remarks>
        /// <param name="id">ID interno do fornecedor.</param>
        [Tags("Fornecedores")]
        [HttpDelete("fornecedores/{id}")]
        public async Task<IActionResult> DeleteFornecedor(string id)
        {
            _logger.LogInformation("[DELETE] Excluindo fornecedor. ID: {id}", id);
            await _dadosService.ExcluirFornecedor(id);
            return Ok(new { mensagem = "Fornecedor deletado com sucesso." });
        }


        /// LOTES


        /// <summary>Lista todos os lotes de matéria-prima.</summary>
        /// <remarks>Retorna os lotes de suprimentos incluindo as métricas ambientais corporativas críticas de cálculo da pegada ecológica por quilograma (kg CO2).</remarks>
        [Tags("Lotes e Componentes")]
        [HttpGet("lotes")]
        public async Task<IActionResult> GetLotes()
        {
            _logger.LogInformation("[GET] Listando lotes.");
            var lotes = await _dadosService.ListarLoteMateriaPrima();
            return Ok(lotes);
        }

        /// <summary>Cadastra um novo lote.</summary>
        /// <remarks>Gera de maneira transacional controlada um identificador incremental do tipo 'contador_lote' antes de persistir o mapeamento de massa de insumos.</remarks>
        /// <param name="lote">Dados do lote.</param>
        [Tags("Lotes e Componentes")]
        [HttpPost("lotes")]
        public async Task<IActionResult> PostLote([FromBody] LoteMateriaPrima lote)
        {
            _logger.LogInformation("[POST] Criando lote.");

            if (lote == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var criado = await _dadosService.CriarLoteMateriaPrima(lote);
            return Ok(new { mensagem = "Lote registrado com sucesso!", lote = criado });
        }

        /// <summary>Exclui um lote pelo ID.</summary>
        /// <remarks>Remove o documento correspondente da coleção "lotes_materia_prima" interrompendo o elo lógico relacional de rastreabilidade do Ledger.</remarks>
        /// <param name="id">ID do lote.</param>
        [Tags("Lotes e Componentes")]
        [HttpDelete("lotes/{id}")]
        public async Task<IActionResult> DeleteLote(string id)
        {
            _logger.LogInformation("[DELETE] Excluindo lote. ID: {id}", id);
            await _dadosService.ExcluirLoteMateriaPrima(id);
            return Ok(new { mensagem = "Lote deletado com sucesso." });
        }


        /// COMPONENTES


        /// <summary>Lista todos os componentes de veículos.</summary>
        /// <remarks>Retorna o acervo de subcomponentes estruturais e peças que se encontram associados a um determinado chassi (VIN).</remarks>
        [Tags("Lotes e Componentes")]
        [HttpGet("componentes")]
        public async Task<IActionResult> GetComponentes()
        {
            _logger.LogInformation("[GET] Listando componentes.");
            var componentes = await _dadosService.ListarVeiculoComponente();
            return Ok(componentes);
        }

        /// <summary>Cadastra um novo componente.</summary>
        /// <remarks>Salva uma nova instância de montagem na coleção "veiculo_componentes" utilizando IDs computados dinamicamente de forma incremental.</remarks>
        /// <param name="componente">Dados do componente.</param>
        [Tags("Lotes e Componentes")]
        [HttpPost("componentes")]
        public async Task<IActionResult> PostComponente([FromBody] VeiculoComponente componente)
        {
            _logger.LogInformation("[POST] Criando componente.");

            if (componente == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var criado = await _dadosService.CriarVeiculoComponente(componente);
            return Ok(new { mensagem = "Componente registrado com sucesso!", componente = criado });
        }

        /// <summary>Exclui um componente pelo ID.</summary>
        /// <remarks>Efetua a limpeza de registro de peça física e encerra o vínculo logístico da linha de produção fabril.</remarks>
        /// <param name="id">ID do componente.</param>
        [Tags("Lotes e Componentes")]
        [HttpDelete("componentes/{id}")]
        public async Task<IActionResult> DeleteComponente(string id)
        {
            _logger.LogInformation("[DELETE] Excluindo componente. ID: {id}", id);
            await _dadosService.ExcluirVeiculoComponente(id);
            return Ok(new { mensagem = "Componente deletado com sucesso." });
        }


        /// AUTENTICAÇÃO


        /// <summary>Cadastra um novo usuário no Firebase.</summary>
        /// <remarks>Valida preventivamente a duplicidade eletrônica de e-mail na nuvem. Trata as credenciais e remove a senha em texto puro do objeto serializado retornado como resposta de segurança.</remarks>
        /// <param name="usuario">Objeto com Nome, Email, Senha e Acesso (opcional).</param>
        /// <response code="200">Usuário criado (senha removida da resposta).</response>
        /// <response code="400">Email ou senha ausentes.</response>
        [Tags("Autenticação")]
        [HttpPost("cadastrar")]
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

        /// <summary>Autentica um usuário com email e senha.</summary>
        /// <remarks>Efetua a varredura e o mapeamento manual reativo nos documentos Firestore, aplicando comparações que evitam gargalos de indexação NoSQL. A senha é ocultada pós-autenticação.</remarks>
        /// <param name="credenciais">Objeto com Email e Senha.</param>
        /// <response code="200">Login bem‑sucedido.</response>
        /// <response code="400">Credenciais não informadas.</response>
        /// <response code="401">Email ou senha incorretos.</response>
        [Tags("Autenticação")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest credenciais)
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

        [Tags("Dashboard")]
        [HttpGet("grafico-emissoes")]
        public async Task<IActionResult> GetDadosGrafico()
        {
            var dados = await _dadosService.ObterDadosGraficoAsync();
            return Ok(dados);
        }

        [Tags("Dashboard")]
        [HttpGet("analises-esg")]
        public async Task<IActionResult> GetDadosAnalisesESG()
        {
            var dados = await _dadosService.ObterDadosAnalisesESGAsync();
            return Ok(dados);
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Senha { get; set; }
        }
    }
}