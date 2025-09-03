using System.Security.Claims;
using ArgusCloud.Application.Comandos;
using ArgusCloud.Application.Contratos;
using ArgusCloud.Application.Interfaces;
using ArgusCloud.Domain.Entities;
using ArgusCloud.Domain.Interfaces.Repositorios;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArgusCloud.API.Controllers
{
    [ApiController]
    [Route("api/ArgusCloud/[controller]")]
    [Authorize]
    public class UsuarioController(IUsuarioRepositorio usuarioRepositorio, IMediator mediator, ITokenServico tokenServico, ILogger<UsuarioController> logger) : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;
        private readonly ITokenServico _tokenServico = tokenServico;

        [HttpPost("atualizarUsuario")]
        public async Task<IActionResult> AtualizarUsuario([FromBody] AtualizarUsuarioContrato contrato)
        {
            var claimIdUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claimIdUsuario != contrato.Id.ToString())
                return Unauthorized("Você só pode atualizar seus próprios dados");

            var usuario = await _usuarioRepositorio.ObterPorIdAsync(contrato.Id);
            if (usuario == null) return NotFound($"Usuario com id: {contrato.Id} não encontrado");

            contrato.Adapt(usuario);

            await _usuarioRepositorio.AtualizarAsync(usuario);

            return Ok();
        }
        [HttpPost("logar")]
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioContrato>> Logar([FromBody] LogarContrato contrato)
        {
            try
            {
                var usuario = await _usuarioRepositorio.ObterPorNomeAsync(contrato.NomeUsuario);

                if (usuario == null) return BadRequest();

                if (!BCrypt.Net.BCrypt.Verify(contrato.Senha, usuario.SenhaHash)) return Unauthorized();

                var token = _tokenServico.GerarTokenFront(usuario.Id, usuario.Nome);

                var usuarioContrato = MapearParaUsuarioContrato(usuario, token);

                return Ok(usuarioContrato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao logar usuário");
                return BadRequest("Falha ao logar");

            }
        }

        //[HttpPost("atualizarToken")]
        //public async Task<ActionResult<string>> RefreshToken([FromBody] LogarContrato contrato)
        //{
        //    try
        //    {

        //        var usuario = await _usuarioRepositorio.ObterPorNomeAsync(contrato.NomeUsuario);

        //        if (usuario == null) return BadRequest();

        //        if (!BCrypt.Net.BCrypt.Verify(contrato.Senha, usuario.SenhaHash)) return Unauthorized();


        //        return ;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erro ao logar usuário");
        //        return BadRequest("Falha ao logar");

        //    }
        //}

        [HttpPost("cadastrarUsuario")]
        public async Task<ActionResult<AdicionarUsuarioContratoResponse>> CadastrarUsuario([FromBody] AdicionarUsuarioContratoResquest contrato, CancellationToken cancellationToken)
        {
            try
            {
                if (await _usuarioRepositorio.ObterPorNomeAsync(contrato.Nome) != null)
                    return Conflict(new ProblemDetails
                    {
                        Title = "Nome de usuário já em uso",
                        Detail = $"O nome '{contrato.Nome}' já está cadastrado.",
                        Status = StatusCodes.Status409Conflict,
                    });

                var comando = new CadastrarComando(contrato.Nome, contrato.Senha);
                var usuarioCadastrado = await _mediator.Send(comando, cancellationToken);

                return usuarioCadastrado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar usuário");
                return BadRequest();
            }

        }
        [HttpGet("verificarAgente/{maquinaId}")]
        [Authorize(Policy = "RequisitoMaquinaId")]
        public async Task<IActionResult> VerificarAgente(string maquinaId)
        {
            try
            {
                if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var usuarioId))
                    return Unauthorized();

                var usuario = await _usuarioRepositorio.ObterPorIdAsync(usuarioId);
                if (usuario != null && Guid.Parse(maquinaId) == usuario.MaquinaId)
                    return Ok();

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar agente com maquinaId: {maquinaId}", maquinaId);
                return BadRequest();
            }

        }

        private static UsuarioContrato MapearParaUsuarioContrato(Usuario usuario, string? token = null)
        {
            var maquina = usuario.Maquina != null
            ? new MaquinaContrato
            {
                Id = usuario.Maquina.Id,
                Nome = usuario.Maquina.Nome,
                SistemaOperacional = usuario.Maquina.SistemaOperacional,
                LocalizacaoMaquina = usuario.Maquina.LocalizacaoMaquina
            }
                : null;
            return new UsuarioContrato
            {
                Nome = usuario.Nome,
                DataExpiracao = usuario.DataExpiracao,
                ExporProcessos = usuario.ExporProcessos,
                IdUsuario = usuario.Id,
                Maquina = maquina,
                PermiteEspelharemProcessos = usuario.PermiteEspelharemProcessos,
                Token = token
            };
        }
    }
}
