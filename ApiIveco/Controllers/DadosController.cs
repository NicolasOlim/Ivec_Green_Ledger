using ApiIveco.Models;
using ApiIveco.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DadosController> _logger;

        public DadosController(DadosService dadosService, ILogger<DadosController> logger)
        {
            _dadosService = dadosService;
            _logger = logger;
        }

        // ==========================================
        // VEÍCULOS
        // ==========================================

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
            _logger.LogInformation("[GET] Listando todos os veículos.");
            var veiculos = await _dadosService.ListarVeiculo();
            _logger.LogInformation("[GET] {count} veículos retornados.", veiculos.Count);
            return Ok(veiculos);
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
            _logger.LogInformation("[GET] Buscando veículo. VIN: {vin}", vin);

            if (string.IsNullOrWhiteSpace(vin))
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var veiculo = await _dadosService.ObterVeiculoPorVin(vin);

            if (veiculo == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            _logger.LogInformation("[GET] Veículo localizado. VIN: {vin}", vin);
            return Ok(new { mensagem = "Veículo encontrado", veiculo });
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
            _logger.LogInformation("[POST] Veículo criado. VIN: {vin}", criado.Vin);
            return Ok(new { mensagem = "Veículo registrado com sucesso!", veiculo = criado });
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
            _logger.LogInformation("[PUT] Atualizando veículo. VIN: {vin}", vin);

            if (veiculo == null || vin != veiculo.Vin)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var atualizado = await _dadosService.AtualizarVeiculo(vin, veiculo);

            if (atualizado == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            _logger.LogInformation("[PUT] Veículo atualizado. VIN: {vin}", vin);
            return Ok(new { mensagem = "Veículo atualizado com sucesso!", veiculo = atualizado });
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
            _logger.LogInformation("[DELETE] Excluindo veículo. VIN: {vin}", vin);

            var existente = await _dadosService.ObterVeiculoPorVin(vin);
            if (existente == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            await _dadosService.ExcluirVeiculo(vin);
            _logger.LogInformation("[DELETE] Veículo excluído. VIN: {vin}", vin);
            return Ok(new { mensagem = "Veículo deletado com sucesso." });
        }

        /// <summary>
        /// Descodifica um VIN e valida se pertence obrigatoriamente à marca IVECO.
        /// </summary>
        [Tags("Veículos")]
        [HttpGet("veiculos/validar-vin/{vin}")]
        public async Task<IActionResult> ValidarVinIveco(string vin)
        {
            _logger.LogInformation("[GET] Validando VIN IVECO: {vin}", vin);

            if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos. O VIN deve ter 17 caracteres." });

            var veiculoIveco = await _dadosService.BuscarEValidarVinIvecoAsync(vin);

            if (veiculoIveco == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            _logger.LogInformation("[GET] VIN IVECO validado: {vin}", vin);
            return Ok(new { mensagem = "Veículo IVECO validado com sucesso!", veiculo = veiculoIveco });
        }

        /// <summary>
        /// Gera um relatório em PDF de todos os veículos cadastrados.
        /// </summary>
        /// <returns>Um arquivo PDF.</returns>
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
            _logger.LogInformation("[GET] PDF gerado com sucesso.");
            return File(pdfBytes, "application/pdf", "Relatorio_Veiculos.pdf");
        }

        // ==========================================
        // FORNECEDORES
        // ==========================================

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
            _logger.LogInformation("[GET] Listando fornecedores.");
            var fornecedores = await _dadosService.ListarFornecedor();
            _logger.LogInformation("[GET] {count} fornecedores retornados.", fornecedores.Count);
            return Ok(fornecedores);
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
            _logger.LogInformation("[GET] Buscando CNPJ: {cnpj}", cnpj);

            if (string.IsNullOrWhiteSpace(cnpj))
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var fornecedor = await _dadosService.BuscarFornecedorPorCnpjAsync(cnpj);

            if (fornecedor == null)
                return NotFound(new { Mensagem = "O recurso solicitado não foi encontrado." });

            _logger.LogInformation("[GET] Fornecedor localizado: {nome}", fornecedor.Nome);
            return Ok(new { mensagem = "Fornecedor localizado com sucesso!", fornecedor });
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
            _logger.LogInformation("[POST] Criando fornecedor.");

            if (fornecedor == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var criado = await _dadosService.CriarFornecedor(fornecedor);
            _logger.LogInformation("[POST] Fornecedor criado. CNPJ: {cnpj}", criado.Cnpj);
            return Ok(new { mensagem = "Fornecedor registrado com sucesso!", fornecedor = criado });
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
            _logger.LogInformation("[DELETE] Excluindo fornecedor. ID: {id}", id);
            await _dadosService.ExcluirFornecedor(id);
            _logger.LogInformation("[DELETE] Fornecedor excluído. ID: {id}", id);
            return Ok(new { mensagem = "Fornecedor deletado com sucesso." });
        }

        // ==========================================
        // LOTES
        // ==========================================

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
            _logger.LogInformation("[GET] Listando lotes.");
            var lotes = await _dadosService.ListarLoteMateriaPrima();
            _logger.LogInformation("[GET] {count} lotes retornados.", lotes.Count);
            return Ok(lotes);
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
            _logger.LogInformation("[POST] Criando lote.");

            if (lote == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var criado = await _dadosService.CriarLoteMateriaPrima(lote);
            _logger.LogInformation("[POST] Lote criado. ID: {id}", criado.Id);
            return Ok(new { mensagem = "Lote registrado com sucesso!", lote = criado });
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
            _logger.LogInformation("[DELETE] Excluindo lote. ID: {id}", id);
            await _dadosService.ExcluirLoteMateriaPrima(id);
            _logger.LogInformation("[DELETE] Lote excluído. ID: {id}", id);
            return Ok(new { mensagem = "Lote deletado com sucesso." });
        }

        // ==========================================
        // COMPONENTES
        // ==========================================

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
            _logger.LogInformation("[GET] Listando componentes.");
            var componentes = await _dadosService.ListarVeiculoComponente();
            _logger.LogInformation("[GET] {count} componentes retornados.", componentes.Count);
            return Ok(componentes);
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
            _logger.LogInformation("[POST] Criando componente.");

            if (componente == null)
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            var criado = await _dadosService.CriarVeiculoComponente(componente);
            _logger.LogInformation("[POST] Componente criado. ID: {id}", criado.Id);
            return Ok(new { mensagem = "Componente registrado com sucesso!", componente = criado });
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
            _logger.LogInformation("[DELETE] Excluindo componente. ID: {id}", id);
            await _dadosService.ExcluirVeiculoComponente(id);
            _logger.LogInformation("[DELETE] Componente excluído. ID: {id}", id);
            return Ok(new { mensagem = "Componente deletado com sucesso." });
        }

        // ==========================================
        // AUTENTICAÇÃO
        // ==========================================

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
            _logger.LogInformation("[POST] Cadastro solicitado. E-mail: {email}", usuario?.Email);

            if (string.IsNullOrWhiteSpace(usuario?.Email) || string.IsNullOrWhiteSpace(usuario?.Senha))
                return BadRequest(new { Mensagem = "Os dados enviados são inválidos." });

            if (string.IsNullOrWhiteSpace(usuario.Acesso))
                usuario.Acesso = "Usuario";

            var criado = await _dadosService.CadastrarUsuario(usuario);
            criado.Senha = "";

            _logger.LogInformation("[POST] Usuário cadastrado. ID: {id}", criado.Id);
            return Ok(new { mensagem = "Usuário cadastrado com sucesso!", usuario = criado });
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
            _logger.LogInformation("[POST] Login OK. Utilizador: {nome} | Acesso: {acesso}", usuario.Nome, usuario.Acesso);
            return Ok(new { mensagem = "Login efetuado com sucesso!", usuario });
        }

        // ==========================================
        // DTO DE LOGIN
        // ==========================================
        public class LoginRequest
        {
            public string Email { get; set; }
            public string Senha { get; set; }
        }
    }
}