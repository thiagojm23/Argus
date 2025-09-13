using System.Text.Json;
using ArgusCloud.Application.Comandos;
using ArgusCloud.Application.Contratos;
using ArgusCloud.Domain.Interfaces.Repositorios;
using ArgusCloud.Infrastructure.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ArgusCloud.API.Hubs
{
    [Authorize]
    public class MonitoramentoHub(IMediator mediator, IUsuarioRepositorio usuarioRepositorio, ILogger<MonitoramentoHub> logger) : Hub
    {
        private readonly IMediator _mediator = mediator;
        private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;
        private readonly ILogger<MonitoramentoHub> _logger = logger;

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();


            if (httpContext == null) return;
            if (!httpContext.Request.Query.TryGetValue("access_token", out var accessToken)) return;

            var maquinaId = httpContext.Request.Headers["maquinaId"].ToString() ?? throw new HubException("Conexão rejeitada por segurança.");
            var tipoCliente = httpContext.Request.Headers["tipoCliente"].ToString() ?? throw new HubException("Conexão rejeitada por segurança."); ;
            var idCliente = httpContext.Request.Headers["idCliente"].ToString() ?? throw new HubException("Conexão rejeitada por segurança.");
            ;

            if (tipoCliente == "agente")
            {
                var comando = new ValidarClaimsTokenComando(accessToken.ToString(), maquinaId, tipoCliente);
                var claimsEhValidas = await _mediator.Send(comando, Context.ConnectionAborted);

                if (!claimsEhValidas)
                {
                    throw new HubException("Conexão rejeitada por segurança.");
                }
                await Groups.AddToGroupAsync(maquinaId, "agentes");
            }
            else if (tipoCliente == "front")
            {
                await Groups.AddToGroupAsync(maquinaId, "clientesFront");

                if (!Guid.TryParse(idCliente, out var guidCliente))
                {
                    throw new ArgumentException("O ID do cliente não é um GUID válido.");
                }

                var cliente = await _usuarioRepositorio.ObterPorIdAsync(guidCliente);
                if (cliente!.ExporProcessos) await Groups.AddToGroupAsync(maquinaId, "clientesFrontExpostos");
            }
        }

        public async Task AtualizarProcessos(string maquinaId, IEnumerable<ProcessoContrato> processosContrato)
        {
            _logger.LogInformation("Processos {objeto}", JsonSerializer.Serialize(processosContrato, JsonConfig.camelCaseOptions));
            if (string.IsNullOrEmpty(maquinaId)) return;

            try
            {
                var comando = new EnviarProcessosComando(maquinaId, processosContrato);
                await _mediator.Send(comando, Context.ConnectionAborted);

                await Clients.Group("clientesFrontExpostos").SendAsync("AtualizarListagemProcessos", new { maquinaId, processos = processosContrato }, Context.ConnectionAborted);
            }
            catch
            {
                await Clients.Caller.SendAsync("Erro", $"Erro ao atualizar processos", Context.ConnectionAborted);
            }

        }

        public async Task<bool> RemoverMaquinaDosExibidos(string maquinaId)
        {
            if (string.IsNullOrEmpty(maquinaId))
            {
                await Clients.Caller.SendAsync("Erro", "O maquinaId não pode ser nulo ou vazio.", Context.ConnectionAborted);
                return false;
            }

            try
            {
                await Groups.RemoveFromGroupAsync(maquinaId, "clientesFrontExpostos");
                return true;
            }
            catch
            {
                await Clients.Caller.SendAsync("Erro", $"Erro ao remover máquina do grupo", Context.ConnectionAborted);
                return false;
            }
        }

        public async Task RestaurarSessaoAoIniciar(string maquinaId)
        {
            await Groups.AddToGroupAsync(maquinaId, "clientesFrontResturarSessao");
        }
    }
}
