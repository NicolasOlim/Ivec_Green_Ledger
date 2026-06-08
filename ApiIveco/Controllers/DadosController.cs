using ApiIveco.Models;
using ApiIveco.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiIveco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DadosController : ControllerBase
    {
        private readonly DadosService _dadosService;

        public DadosController(DadosService dadosService)
        {
            _dadosService = dadosService;
        }

        /// <summary>
        /// Retorna a lista completa de veiculos cadastrados no banco de dados.
        /// </summary>
        /// <returns>Uma lista contendo todos os veiculos.</returns>
        /// <response code="200">Retorna a lista de veiculos com sucesso.</response>
        /// <response code="400">Se houver algum problema com os parâmetros de consulta.</response>
        /// <response code="404">Se a coleção de veiculos não for encontrada.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor (ex: falha no Firebase).</response>
        [HttpGet("veiculos")]
        public async Task<IActionResult> GetVeiculos()
        {
            try
            {
                var veiculos = await _dadosService.ListarVeiculo();
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
        [HttpGet("veiculos/{vin}")]
        public async Task<IActionResult> GetVeiculoByVin(string vin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vin))
                    return BadRequest(new { Erro = "VIN obrigatório", Mensagem = "O VIN não pode ser vazio." });

                var veiculo = await _dadosService.ObterVeiculoPorVin(vin);
                if (veiculo == null)
                    return NotFound(new { Erro = "Não encontrado", Mensagem = $"Nenhum veículo encontrado com o VIN: {vin}" });

                return Ok(new { mensagem = "Veículo encontrado", veiculo });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao obter veículo"); }
        }

        /// <summary>
        /// Cria e salva um novo veiculo no banco de dados.
        /// </summary>
        /// <param name="veiculo">O objeto JSON contendo os dados do veiculo a ser criado.</param>
        /// <returns>O veiculo recém-criado junto com a rota para acessá-lo.</returns>
        /// <response code="201">Retorna o veiculo criado e o cabeçalho Location com a URI de acesso.</response>
        /// <response code="400">Se os dados enviados forem nulos ou o nome do veiculo estiver vazio.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar salvar.</response>
        [HttpPost("veiculos")]
        public async Task<IActionResult> PostVeiculo([FromBody] Veiculo veiculo)
        {
            if (veiculo == null) return BadRequest(new { Erro = "Dados inválidos", Mensagem = "Requisição nula." });
            if (string.IsNullOrWhiteSpace(veiculo.Vin)) return BadRequest(new { Erro = "Campo Obrigatório", Mensagem = "VIN vazio." });

            try
            {
                var todos = await _dadosService.ListarVeiculo();
                if (todos.Any(v => v.Vin.Equals(veiculo.Vin, StringComparison.OrdinalIgnoreCase)))
                    return Conflict(new { Erro = "Duplicado", Mensagem = $"Veículo com VIN '{veiculo.Vin}' já cadastrado!" });

                var criado = await _dadosService.CriarVeiculo(veiculo);
                return Ok(new { mensagem = "Veículo registrado com sucesso!", veiculo = criado });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao criar veículo"); }
        }

        /// <summary>
        /// Exclui um veiculo do banco de dados.
        /// </summary>
        /// <param name="vin">O VIN do veiculo que será excluído.</param>
        /// <returns>Uma mensagem de sucesso confirmando a exclusão.</returns>
        /// <response code="200">Se o veiculo for excluído com sucesso.</response>
        /// <response code="404">Se o veiculo com o ID especificado não for encontrado.</response>
        /// <response code="500">Se ocorrer um erro interno no servidor ao tentar excluir.</response>
        [HttpDelete("veiculos/{vin}")]
        public async Task<IActionResult> DeleteVeiculo(string vin)
        {
            try
            {
                var existente = await _dadosService.ObterVeiculoPorVin(vin);
                if (existente == null) return NotFound(new { Erro = "Não encontrado", Mensagem = "Veículo não encontrado." });

                await _dadosService.ExcluirVeiculo(vin);
                return Ok(new { mensagem = "Veículo Deletado com Sucesso" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao deletar veículo"); }
        }

        /// <summary>
        /// Descodifica um VIN e valida se pertence obrigatoriamente à marca IVECO.
        /// </summary>
        [HttpGet("veiculos/validar-vin/{vin}")]
        public async Task<IActionResult> ValidarVinIveco(string vin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
                    return BadRequest(new { Erro = "VIN Inválido", Mensagem = "O VIN deve conter exatamente 17 caracteres." });

                var veiculoIveco = await _dadosService.BuscarEValidarVinIvecoAsync(vin);

                if (veiculoIveco == null)
                    return NotFound(new { Erro = "Não encontrado", Mensagem = "Não foi possível descodificar os dados deste VIN." });

                return Ok(new { mensagem = "Veículo IVECO validado com sucesso!", veiculo = veiculoIveco });
            }
            catch (Exception ex)
            {
                // Se a exceção for o nosso bloqueio de marca, retornamos um erro 403 (Proibido)
                if (ex.Message.Contains("Apenas veículos IVECO são permitidos"))
                {
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
        [HttpGet("fornecedores")]
        public async Task<IActionResult> GetFornecedores()
        {
            try
            {
                var fornecedores = await _dadosService.ListarFornecedor();
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
        [HttpGet("fornecedores/buscar-cnpj/{cnpj}")]
        public async Task<IActionResult> GetFornecedorCnpj(string cnpj)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cnpj))
                    return BadRequest(new { Erro = "CNPJ obrigatório", Mensagem = "Digite o CNPJ da empresa." });

                var proveedores = await _dadosService.BuscarFornecedorPorCnpjAsync(cnpj);

                if (proveedores == null)
                    return NotFound(new { Erro = "Não encontrado", Mensagem = "CNPJ não encontrado na base da Receita Federal." });

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
        [HttpPost("fornecedores")]
        public async Task<IActionResult> PostFornecedor([FromBody] Fornecedor fornecedor)
        {
            if (fornecedor == null) return BadRequest("Dados do fornecedor não podem ser nulos.");
            try
            {
                var criado = await _dadosService.CriarFornecedor(fornecedor);
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
        [HttpDelete("fornecedores/{id}")]
        public async Task<IActionResult> DeleteFornecedor(string id)
        {
            try
            {
                await _dadosService.ExcluirFornecedor(id);
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
        [HttpGet("lotes")]
        public async Task<IActionResult> GetLotes()
        {
            try
            {
                var lotes = await _dadosService.ListarLoteMateriaPrima();
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
        [HttpPost("lotes")]
        public async Task<IActionResult> PostLote([FromBody] LoteMateriaPrima lote)
        {
            if (lote == null) return BadRequest("Dados do lote não podem ser nulos.");
            try
            {
                var criado = await _dadosService.CriarLoteMateriaPrima(lote);
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
        [HttpDelete("lotes/{id}")]
        public async Task<IActionResult> DeleteLote(string id)
        {
            try
            {
                await _dadosService.ExcluirLoteMateriaPrima(id);
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
        [HttpGet("componentes")]
        public async Task<IActionResult> GetComponentes()
        {
            try
            {
                var componentes = await _dadosService.ListarVeiculoComponente();
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
        [HttpPost("componentes")]
        public async Task<IActionResult> PostComponente([FromBody] VeiculoComponente componente)
        {
            if (componente == null) return BadRequest("Dados do componente não podem ser nulos.");
            try
            {
                var criado = await _dadosService.CriarVeiculoComponente(componente);
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
        [HttpDelete("componentes/{id}")]
        public async Task<IActionResult> DeleteComponente(string id)
        {
            try
            {
                await _dadosService.ExcluirVeiculoComponente(id);
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
        [HttpPost("cadastrar")]
        public async Task<IActionResult> Cadastrar([FromBody] ApiIveco.Models.Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Email) || string.IsNullOrWhiteSpace(usuario.Senha))
                return BadRequest(new { Erro = "Dados Inválidos", Mensagem = "E-mail e senha são obrigatórios." });

            try
            {
                // Se não enviar perfil, vira "Usuario" padrão. Se enviar "Admin", vira Admin.
                if (string.IsNullOrWhiteSpace(usuario.Acesso))
                    usuario.Acesso = "Usuario";

                var criado = await _dadosService.CadastrarUsuario(usuario);

                // Oculta a senha antes de devolver a resposta por segurança
                criado.Senha = "";
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest credenciais)
        {
            if (string.IsNullOrWhiteSpace(credenciais.Email) ||
                string.IsNullOrWhiteSpace(credenciais.Senha))
                return BadRequest(new
                {
                    Erro = "Dados Inválidos",
                    Mensagem = "Informe e-mail e senha."
                });

            try
            {
                var usuario = await _dadosService.FazerLogin(
                    credenciais.Email, credenciais.Senha);

                if (usuario == null)
                    return Unauthorized(new
                    {
                        Erro = "Acesso Negado",
                        Mensagem = "E-mail ou senha incorretos."
                    });

                usuario.Senha = "";
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
            Console.WriteLine($"{contexto}: {ex.Message}");

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