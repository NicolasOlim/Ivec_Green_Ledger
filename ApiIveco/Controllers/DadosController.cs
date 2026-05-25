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
        /// <param name="id">O identificador único do veiculo (String).</param>
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
        /// <param name="jogo">O objeto JSON contendo os dados do veiculo a ser criado.</param>
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
        /// <param name="id">O VIN do veiculo que será excluído.</param>
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
        /// Cria e salva um novo fornecedor no banco de dados.
        /// </summary>
        /// <param name="jogo">O objeto JSON contendo os dados do fornecedor a ser criado.</param>
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
        /// <param name="jogo">O objeto JSON contendo os dados do lote a ser criado.</param>
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
        /// <param name="jogo">O objeto JSON contendo os dados do componente a ser criado.</param>
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
    }
}