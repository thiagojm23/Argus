using ArgusCloud.Application.Comandos;
using ArgusCloud.Application.Contratos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ArgusCloud.API.Hubs
{
    [Authorize]
    public class MonitoramentoHub(IMediator mediator) : Hub
    {
        private readonly IMediator _mediator = mediator;

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();


            if (httpContext == null) return;
            if (!httpContext.Request.Query.TryGetValue("access_token", out var accessToken)) return;

            var maquinaId = httpContext.Request.Headers["maquinaId"].ToString();
            var tipoCliente = httpContext.Request.Headers["tipoCliente"].ToString();

            if (tipoCliente == "agente")
            {
                var comando = new ValidarClaimsTokenComando(accessToken.ToString(), maquinaId, tipoCliente);
                var claimsEhValidas = await _mediator.Send(comando, Context.ConnectionAborted);

                var claimMaquinaId = httpContext.User.FindFirst("maquinaId")?.Value;
                if (!claimsEhValidas)
                {
                    throw new HubException("Conexão rejeitada por segurança.");
                }
                await Groups.AddToGroupAsync(maquinaId, "agentes");
            }
            else if (tipoCliente == "front")
            {
                await Groups.AddToGroupAsync(maquinaId, "clientesFront");
            }
        }

        public async Task AtualizarProcessos(string maquinaId, IEnumerable<ProcessoContrato> processosContrato)
        {
            if (string.IsNullOrEmpty(maquinaId)) return;

            var comando = new EnviarProcessosComando(maquinaId, processosContrato);
            await _mediator.Send(comando, Context.ConnectionAborted);

            await Clients.Group(maquinaId).SendAsync("AtualizarListagemProcessos", "agente", processosContrato, Context.ConnectionAborted);
        }
    }
}
