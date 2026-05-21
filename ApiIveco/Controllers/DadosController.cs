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
    public class VeiculosController : ControllerBase
    {
        private readonly DadosService _dadosService;

        public VeiculosController(DadosService dadosService)
        {
            _dadosService = dadosService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var veiculos = await _dadosService.ListarVeiculo();
                return Ok(veiculos);
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Erro ao listar veículos");
            }
        }

        [HttpGet("{vin}")]
        public async Task<IActionResult> GetByVin(string vin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vin))
                {
                    return BadRequest(new { Erro = "VIN obrigatório", Mensagem = "O VIN não pode ser nulo ou vazio." });
                }

                // Certifique-se de criar este método no seu DadosService
                var veiculo = await _dadosService.ObterVeiculoPorVin(vin);

                if (veiculo == null)
                {
                    return NotFound(new { Erro = "Não encontrado", Mensagem = $"Nenhum veículo encontrado com o VIN: {vin}" });
                }

                return Ok(new { mensagem = "Veículo encontrado: ", veiculo });
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Erro ao obter veículo");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Veiculo veiculo)
        {
            if (veiculo == null)
            {
                return BadRequest(new { Erro = "Dados inválidos", Mensagem = "O corpo da requisição não pode ser nulo." });
            }

            if (string.IsNullOrWhiteSpace(veiculo.Vin))
            {
                return BadRequest(new { Erro = "Campo Obrigatório", Mensagem = "O VIN do veículo não pode ser vazio." });
            }

            var todosOsVeiculos = await _dadosService.ListarVeiculo();
            bool veiculoJaExiste = todosOsVeiculos.Any(v => v.Vin.Equals(veiculo.Vin, StringComparison.OrdinalIgnoreCase));

            if (veiculoJaExiste)
            {
                return Conflict(new { Erro = "Duplicado", Mensagem = $"O veículo com VIN '{veiculo.Vin}' já está cadastrado!" });
            }

            try
            {
                var veiculoCriado = await _dadosService.CriarVeiculo(veiculo);
                return CreatedAtAction(nameof(GetByVin), new { vin = veiculoCriado.Vin }, veiculoCriado);
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Erro ao criar veículo");
            }
        }

       

        [HttpDelete("{vin}")]
        public async Task<IActionResult> Delete(string vin)
        {
            try
            {
                var existente = await _dadosService.ObterVeiculoPorVin(vin);
                if (existente == null)
                {
                    return NotFound(new { Erro = "Não encontrado", Mensagem = $"Nenhum veículo encontrado com o VIN: {vin}" });
                }

                await _dadosService.ExcluirVeiculo(vin);
                return Ok(new { mensagem = "Veículo Deletado com Sucesso" });
            }
            catch (Exception ex)
            {
                return TratarErro(ex, "Erro ao deletar veículo");
            }
        }

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