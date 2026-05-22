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
    [Route("api/[controller]")] // Rota base: api/dados
    [ApiController]
    public class DadosController : ControllerBase
    {
        private readonly DadosService _dadosService;

        public DadosController(DadosService dadosService)
        {
            _dadosService = dadosService;
        }

        // ==========================================
        // 1. VEÍCULOS
        // ==========================================
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

                return Ok(new { mensagem = "Veículo encontrado: ", veiculo });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao obter veículo"); }
        }

        [HttpPost("veiculos")]
        public async Task<IActionResult> PostVeiculo([FromBody] Veiculo veiculo)
        {
            if (veiculo == null) return BadRequest(new { Erro = "Dados inválidos", Mensagem = "Requisicao nula." });
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

        // ==========================================
        // 2. FORNECEDORES
        // ==========================================
        [HttpGet("fornecedores")]
        public async Task<IActionResult> GetFornecedores()
        {
            try
            {
                // var fornecedores = await _dadosService.ListarFornecedores();
                // return Ok(fornecedores);
                return Ok("Método de listar fornecedores em construção no Service.");
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao listar fornecedores"); }
        }

        [HttpPost("fornecedores")]
        public async Task<IActionResult> PostFornecedor([FromBody] Fornecedor fornecedor)
        {
            if (fornecedor == null) return BadRequest("Dados do fornecedor não podem ser nulos.");
            try
            {
                // await _dadosService.CriarFornecedor(fornecedor); 
                return Ok(new { mensagem = "Fornecedor registrado com sucesso!" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao criar fornecedor"); }
        }

        [HttpDelete("fornecedores/{id}")]
        public async Task<IActionResult> DeleteFornecedor(string id)
        {
            try
            {
                // await _dadosService.ExcluirFornecedor(id);
                return Ok(new { mensagem = "Fornecedor Deletado com Sucesso" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao deletar fornecedor"); }
        }

        // ==========================================
        // 3. LOTES
        // ==========================================
        [HttpGet("lotes")]
        public async Task<IActionResult> GetLotes()
        {
            try
            {
                // var lotes = await _dadosService.ListarLotes();
                // return Ok(lotes);
                return Ok("Método de listar lotes em construção no Service.");
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao listar lotes"); }
        }

        [HttpPost("lotes")]
        public async Task<IActionResult> PostLote([FromBody] LoteMateriaPrima lote)
        {
            if (lote == null) return BadRequest("Dados do lote não podem ser nulos.");
            try
            {
                // await _dadosService.CriarLote(lote); 
                return Ok(new { mensagem = "Lote registrado com sucesso!" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao criar lote"); }
        }

        [HttpDelete("lotes/{id}")]
        public async Task<IActionResult> DeleteLote(string id)
        {
            try
            {
                // await _dadosService.ExcluirLote(id);
                return Ok(new { mensagem = "Lote Deletado com Sucesso" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao deletar lote"); }
        }

        // ==========================================
        // 4. COMPONENTES
        // ==========================================
        [HttpGet("componentes")]
        public async Task<IActionResult> GetComponentes()
        {
            try
            {
                // var componentes = await _dadosService.ListarComponentes();
                // return Ok(componentes);
                return Ok("Método de listar componentes em construção no Service.");
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao listar componentes"); }
        }

        [HttpPost("componentes")]
        public async Task<IActionResult> PostComponente([FromBody] VeiculoComponente componente)
        {
            if (componente == null) return BadRequest("Dados do componente não podem ser nulos.");
            try
            {
                // await _dadosService.CriarComponente(componente); 
                return Ok(new { mensagem = "Componente registrado com sucesso!" });
            }
            catch (Exception ex) { return TratarErro(ex, "Erro ao criar componente"); }
        }

        [HttpDelete("componentes/{id}")]
        public async Task<IActionResult> DeleteComponente(string id)
        {
            try
            {
                // await _dadosService.ExcluirComponente(id);
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