using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace Argus.Agent
{
    public class ListenersSignalR(HubConnection? hubConnection, string tokenAutenticacao, ILogger<Worker> logger)
    {
        private HubConnection? _hubConnection = hubConnection;
        private readonly string _tokenAutenticacao = tokenAutenticacao;
        private readonly ILogger<Worker> _logger = logger;
        private readonly HttpClient _httpClient = new();


        public async Task<HubConnection> RegistrarListeners(string hubUri, string apiBaseUri, string maquinaId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{apiBaseUri}/Usuario/verificarAgente/{maquinaId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAutenticacao);

            var response = await _httpClient.SendAsync(request);


            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Retorno: " + response.StatusCode);
                _logger.LogError("Erro ao verificar agente com maquinaId: {maquinaId}", maquinaId);
                throw new Exception();
            }
            var dadosResposta = await response.Content.ReadFromJsonAsync<VerificarAgenteContrato>() ?? throw new Exception();

            Worker.processosVisiveisParaRede = dadosResposta.ExporProcessos;

            var queryParms = $"?maquinaId={maquinaId}&tipoCliente=agente";

            _logger.LogInformation("token: {token}", _tokenAutenticacao);

            _hubConnection = new HubConnectionBuilder().WithUrl($"{hubUri}{queryParms}", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_tokenAutenticacao)!;
            }).WithAutomaticReconnect().Build();

            _hubConnection.On<bool>("RestaurarSessaoAoIniciar", (isEnabled) =>
            {
                _logger.LogInformation("Recebido comando para {status} a funcionalidade de reiniciar apps.", isEnabled ? "habilitar" : "desabilitar");

                if (isEnabled)
                    HabilitarAbrirProcessosAoIniciar();
                else
                    DesabilitarAbrirProcessosAoIniciar();
            });
            _hubConnection.On<bool>("AlterarCompartilhamentoDetalhado", (novoValor) =>
            {
                _logger.LogInformation("Recebido comando para alterar compartilhamento detalhado, novo valor: {valor}", novoValor);

                Worker.enviarProcessosDetalhados = novoValor;
            });
            _hubConnection.On<bool>("AlterarProcessosVisiveisParaRede", (novoValor) =>
            {
                _logger.LogInformation("Recebido comando para alterar processos visíveis para rede, novo valor: {valor}", novoValor);

                Worker.processosVisiveisParaRede = novoValor;
            });
            _hubConnection.On("AtualizarDashAgente", () =>
            {
                _logger.LogInformation("Recebido comando para atualizar processos visíveis para rede (agente)");

                Worker.novoObservadorAdicionado = true;
            });

            return _hubConnection;
        }

        private static void HabilitarAbrirProcessosAoIniciar()
        {
            File.WriteAllText(Path.Combine(GerenciadorCredencial.BuscarCaminhoDados(), "resetar_processos.json"), string.Empty);
        }

        private static void DesabilitarAbrirProcessosAoIniciar()
        {
            var caminhoArquivo = Path.Combine(GerenciadorCredencial.BuscarCaminhoDados(), "resetar_processos.json");
            if (File.Exists(caminhoArquivo))
                File.Delete(caminhoArquivo);
        }

        //private static string RetornarMaquinaIdUsuarioToken(string token)
        //{
        //    try
        //    {
        //        var handler = new JwtSecurityTokenHandler();
        //        var jwt = handler.ReadJwtToken(token);

        //        var claim = jwt.Claims.FirstOrDefault(c =>
        //            c.Type == "maquinaId");

        //        return claim?.Value ?? string.Empty;
        //    }
        //    catch
        //    {
        //        return string.Empty;
        //    }
        //}
    }
}
