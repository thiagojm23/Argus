using System.Security.Claims;
using System.Text.Json;
using ArgusCloud.Application.Comandos;
using ArgusCloud.Application.Contratos;
using ArgusCloud.Application.Interfaces;
using ArgusCloud.Domain.Interfaces.Repositorios;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ArgusCloud.API.Hubs
{
    [Authorize]
    public class MonitoramentoHub(IMediator mediator, IUsuarioRepositorio usuarioRepositorio, IGerenciadorGruposServico gerenciadorGruposServico, ILogger<MonitoramentoHub> logger) : Hub
    {
        private readonly IMediator _mediator = mediator;
        private readonly IUsuarioRepositorio _usuarioRepositorio = usuarioRepositorio;
        private readonly IGerenciadorGruposServico _gerenciadorGruposServico = gerenciadorGruposServico;
        private readonly ILogger<MonitoramentoHub> _logger = logger;

        public override async Task OnConnectedAsync()
        {
            string maquinaId;
            string tipoCliente;
            string tokenAgente;

            var httpContext = Context.GetHttpContext() ?? throw new HubException("Conexão rejeitada por segurança.");

            tipoCliente = httpContext.Request.Query["tipoCliente"].ToString();
            maquinaId = httpContext.Request.Query["maquinaId"].ToString();

            if (tipoCliente == "agente")
            {
                var accessToken = await httpContext.GetTokenAsync("access_token");
                tokenAgente = accessToken ?? "";
            }
            else
            {
                var accessToken = httpContext.Request.Cookies["cookie-signal-r"];
                tokenAgente = accessToken ?? "";
            }


            _logger.LogInformation("Dados conexão SignalR: {maq}, {token}, {tipo}", maquinaId, tokenAgente, tipoCliente);

            if (new[] { tokenAgente, maquinaId, tipoCliente }.Any(string.IsNullOrEmpty))
                throw new HubException("Conexão rejeitada por segurança.");


            if (tipoCliente == "agente")
            {
                var comando = new ValidarTokenComando(tokenAgente.ToString(), maquinaId, tipoCliente);
                var claimsEhValidas = await _mediator.Send(comando, Context.ConnectionAborted);

                if (!claimsEhValidas)
                {
                    throw new HubException("Conexão rejeitada por segurança.");
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, $"agentes");
            }
            else if (tipoCliente == "front")
            {
                var comando = new ValidarClaimsTokenComando(tokenAgente.ToString(), maquinaId, tipoCliente);
                var claimsEhValidas = await _mediator.Send(comando, Context.ConnectionAborted);

                if (!claimsEhValidas)
                {
                    throw new HubException("Conexão rejeitada por segurança.");
                }

                if (!Guid.TryParse(maquinaId, out var guidMaquina))
                {
                    throw new ArgumentException("O ID do cliente não é um GUID válido.");
                }

                var cliente = await _usuarioRepositorio.ObterPorMaquinaIdAsync(guidMaquina) ?? throw new ArgumentException("Cliente não cadastrado");

                await Groups.AddToGroupAsync(Context.ConnectionId, "clientesFront");

                await Clients.Group($"agentes").SendAsync("AtualizarDashAgente", Context.ConnectionAborted);

                //if (!cliente.ExporProcessos)
                //{
                //    await Groups.AddToGroupAsync(Context.ConnectionId, "clientesFrontExpostos");
                //}
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var tipoCliente = Context.User!.FindFirstValue("tipoCliente")!;
            var maquinaId = Context.User!.FindFirstValue("maquinaId")!;

            if (tipoCliente == "front")
            {
                var grupos = _gerenciadorGruposServico.RemoverDeTodosOsGrupos(Context.ConnectionId);
                Console.WriteLine(JsonSerializer.Serialize(grupos));

                foreach (var grupo in grupos)
                {
                    var quantidadeObservadores = _gerenciadorGruposServico.QtdObservadoresAgente(grupo);
                    Console.WriteLine(grupo);
                    var maquinaIdObservada = grupo.Split('_').Last();

                    if (quantidadeObservadores == 0) await Clients.User(maquinaIdObservada).SendAsync("AlterarCompartilhamentoDetalhado", false);
                }
            }
        }

        public async Task AtualizarProcessosDetalhados(IEnumerable<ProcessoContrato> processosContrato)
        {
            try
            {
                var maquinaIdObservada = Context.User!.FindFirst("maquinaId")!.Value;

                //var usuario = await _usuarioRepositorio.ObterPorMaquinaIdAsync(Guid.Parse(maquinaIdObservada)) ?? throw new Exception();

                //if (usuario.PermiteEspelharemProcessos)
                //{
                //    var comando = new AtualizarProcessosEmMemoriaComando(maquinaIdObservada, processosContrato);
                //    await _mediator.Send(comando, Context.ConnectionAborted);

                //}

                await Clients.Group($"agente_{maquinaIdObservada}").SendAsync("AtualizarProcessosDetalhados", new { maquinaIdObservada, processos = processosContrato }, Context.ConnectionAborted);
            }
            catch
            {
                await Clients.Caller.SendAsync("Erro", $"Erro ao atualizar processos", Context.ConnectionAborted);
            }
        }

        public async Task AtualizarProcessosDash(IEnumerable<string> top4Processos)
        {
            try
            {
                Console.WriteLine("oi");
                var maquinaIdObservada = Context.User!.FindFirst("maquinaId")!.ToString();

                await Clients.Group("clientesFront").SendAsync("AtualizarProcessosDash", new { maquinaIdObservada, processos = top4Processos }, Context.ConnectionAborted);
            }
            catch
            {
                await Clients.Caller.SendAsync("Erro", $"Erro ao atualizar processos", Context.ConnectionAborted);
            }
        }

        public async Task<bool> AlterarProcessosVisiveisParaRede(string maquinaId, bool novoValor)
        {
            if (string.IsNullOrEmpty(maquinaId))
            {
                await Clients.Caller.SendAsync("Erro", "O maquinaId não pode ser nulo ou vazio.", Context.ConnectionAborted);
                return false;
            }

            try
            {
                var usuario = await _usuarioRepositorio.ObterPorMaquinaIdAsync(Guid.Parse(maquinaId));

                if (usuario == null)
                {
                    _logger.LogError("Erro ao obter usuário com maquina id: {id}", maquinaId);
                    return false;
                }

                usuario.ExporProcessos = novoValor;

                await _usuarioRepositorio.AtualizarAsync(usuario);

                await Clients.User(maquinaId).SendAsync("AlterarProcessosVisiveisParaRede", novoValor);
                return true;
            }
            catch
            {
                await Clients.Caller.SendAsync("Erro", $"Erro ao remover máquina do grupo", Context.ConnectionAborted);
                return false;
            }
        }

        public async Task<bool> AlterarCompartilhamentoDetalhado(string maquinaIdObservada, bool novoValor)
        {
            if (string.IsNullOrEmpty(maquinaIdObservada))
            {
                await Clients.Caller.SendAsync("Erro", "O maquinaIdObservada não pode ser nulo ou vazio.", Context.ConnectionAborted);
                return false;
            }
            try
            {
                var maquinaIdObservador = Context.User?.FindFirst("maquinaId")?.ToString().Trim();

                if (string.IsNullOrEmpty(maquinaIdObservador))
                {
                    await Clients.Caller.SendAsync("Erro", "O maquinaIdObservador não pode ser nulo ou vazio.", Context.ConnectionAborted);
                    return false;
                }
                if (novoValor)
                {
                    _gerenciadorGruposServico.AdicionarNoGrupo(Context.ConnectionId, $"agente_{maquinaIdObservada}");
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"agente_{maquinaIdObservada}");

                    var quantidadeObservadores = _gerenciadorGruposServico.QtdObservadoresAgente($"agente_{maquinaIdObservada}");

                    if (quantidadeObservadores == 1) await Clients.User(maquinaIdObservada).SendAsync("AlterarCompartilhamentoDetalhado", true);
                }
                else
                {
                    _gerenciadorGruposServico.RemoverDoGrupo(Context.ConnectionId, $"agente_{maquinaIdObservada}");
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"agente_{maquinaIdObservada}");

                    var quantidadeObservadores = _gerenciadorGruposServico.QtdObservadoresAgente($"agente_{maquinaIdObservada}");

                    if (quantidadeObservadores == 0) await Clients.User(maquinaIdObservada).SendAsync("AlterarCompartilhamentoDetalhado", false);
                }

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
