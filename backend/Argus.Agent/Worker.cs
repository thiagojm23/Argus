using System.ComponentModel;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;


namespace Argus.Agent;

public class Worker(ILogger<Worker> logger, IConfiguration configuracao) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConfiguration _configuracao = configuracao;
    private HubConnection? _hubConnection;
    private readonly string? _tokenAutenticacao = GerenciadorCredencial.LerToken();
    private readonly string? _maquinaId = RetornarMaquinaIdUsuarioToken(GerenciadorCredencial.LerToken());
    private readonly Dictionary<int, TimeSpan> _tempoCpuModoUsuario = [];
    private readonly Dictionary<int, TimeSpan> _tempoCpuModoPrivilegiado = [];
    private readonly Dictionary<int, TimeSpan> _tempoCpuTotal = [];
    private readonly Dictionary<int, DateTime> _ultimoTempoLido = [];

    private static List<string> top4Processos = [];
    public static bool processosVisiveisParaRede = false;
    public static bool enviarProcessosDetalhados = false;
    public static bool novoObservadorAdicionado = true;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenAutenticacao))
        {
            _logger.LogError("Token de autenticação não encontrado. Execute o agente com o parâmetro --register para configurar.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_maquinaId))
        {
            _logger.LogError("Erro ao obter id da maquina");
            throw new Exception();
        }

        await ConfigurarSignalR(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    if (processosVisiveisParaRede)
                    {
                        var top20Processos = BuscarTop20ProcessosSistema();

                        await EnviarProcessosDashboard(top20Processos, stoppingToken);

                        if (enviarProcessosDetalhados)
                        {
                            Console.WriteLine("Caiu aqui");
                            await EnviarProcessosDetalhados(top20Processos, stoppingToken);
                        }

                        var caminhoArquivo = Path.Combine(GerenciadorCredencial.BuscarCaminhoDados(), "resetar_processos.json");
                        if (File.Exists(caminhoArquivo)) HabilitarAbrirProcessosAoIniciar();

                    }
                }
                else
                {
                    _logger.LogWarning("Conexão com o SignalR não está ativa. Encerrando agente");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao enviar os dados dos processos.");
                return;
            }
            await Task.Delay(500, stoppingToken);
        }
    }

    private async Task ConfigurarSignalR(CancellationToken tokenCancelamento)
    {
        var hubUri = _configuracao["Backend:SignalRHubUrl"];
        var apiBaseUri = _configuracao["Backend:ApiBaseUrl"];

        if (string.IsNullOrEmpty(hubUri) || string.IsNullOrEmpty(apiBaseUri)) return;

        var listenersSignalR = new ListenersSignalR(_hubConnection, _tokenAutenticacao!, _logger);

        try
        {
            _hubConnection = await listenersSignalR.RegistrarListeners(hubUri, apiBaseUri, _maquinaId!);
            _logger.LogInformation("Conectando ao SiganlR Hub");
            await _hubConnection.StartAsync(tokenCancelamento);
            _logger.LogInformation("Conectado com sucesso ao SignalR Hub");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao conectar ao SignalR Hub");
        }

    }

    private List<ProcessoInfo> BuscarTop20ProcessosSistema()
    {
        var topProcessos = Process.GetProcesses().OrderByDescending(p => p.WorkingSet64).ToList();
        var processosAgrupados = new Dictionary<string, ProcessoInfo>();

        var processosAtuaisIds = new HashSet<int>();

        var contador = 0;
        while (processosAgrupados.Count < 20)
        {
            try
            {
                string nomeExecutavel = topProcessos[contador].MainModule?.FileName ?? "desconhecido";

                if (processosAgrupados.TryGetValue(nomeExecutavel, out ProcessoInfo? processoJaListado))
                {
                    var (usoCpuModoUsuario, usoCpuModoPrivilegiado, usoCpuTotal) = CalcularUsoCpu(topProcessos[contador]);

                    processoJaListado.SubProcessos.Add(new ProcessoInfoBase
                    {
                        Id = topProcessos[contador].Id,
                        Nome = RetornarNomeProcesso(topProcessos[contador]),
                        NomeExecutavel = nomeExecutavel,
                        NumeroThreads = topProcessos[contador].Threads.Count,
                        MemoriaUsoMB = topProcessos[contador].WorkingSet64 / (1024 * 1024),
                        MemoriaVirtualUsoMB = topProcessos[contador].VirtualMemorySize64 / (1024 * 1024),
                        UsoCpuModoUsuario = usoCpuModoUsuario,
                        UsoCpuModoPrivilegiado = usoCpuModoPrivilegiado,
                        UsoCpuTotal = usoCpuTotal
                    });

                    processoJaListado.MemoriaUsoMB += topProcessos[contador].WorkingSet64 / (1024 * 1024);
                    processoJaListado.NumeroThreads += topProcessos[contador].Threads.Count;
                    processoJaListado.UsoCpuModoPrivilegiado += usoCpuModoPrivilegiado;
                    processoJaListado.MemoriaVirtualUsoMB += topProcessos[contador].VirtualMemorySize64 / (1024 * 1024);
                    processoJaListado.UsoCpuModoUsuario += usoCpuModoUsuario;
                    processoJaListado.UsoCpuTotal += usoCpuTotal;
                }
                else
                {
                    processosAtuaisIds.Add(topProcessos[contador].Id);

                    var (usoCpuModoUsuario, usoCpuModoPrivilegiado, usoCpuTotal) = CalcularUsoCpu(topProcessos[contador]);

                    processosAgrupados[nomeExecutavel] = new ProcessoInfo
                    {
                        Id = topProcessos[contador].Id,
                        Nome = RetornarNomeProcesso(topProcessos[contador]),
                        NomeExecutavel = nomeExecutavel,
                        NumeroThreads = topProcessos[contador].Threads.Count,
                        MemoriaUsoMB = topProcessos[contador].WorkingSet64 / (1024 * 1024),
                        MemoriaVirtualUsoMB = topProcessos[contador].VirtualMemorySize64 / (1024 * 1024),
                        UsoCpuModoUsuario = usoCpuModoUsuario,
                        UsoCpuModoPrivilegiado = usoCpuModoPrivilegiado,
                        UsoCpuTotal = usoCpuTotal,
                        SubProcessos = [],
                    };
                }
            }
            catch (Win32Exception)
            {
                continue;
            }
            finally
            {
                contador++;
            }
        }

        LimparProcessosAntigos(processosAtuaisIds);

        return [.. processosAgrupados.Values.OrderByDescending(processo => processo.MemoriaUsoMB)];
    }
    private async Task EnviarProcessosDashboard(List<ProcessoInfo> top20Processos, CancellationToken cancellationToken)
    {
        var novos4Processos = top20Processos.Select(processo => processo.Nome).Take(4).ToList();
        if (novoObservadorAdicionado || !novos4Processos.SequenceEqual(top4Processos))
        {
            top4Processos = novos4Processos;
            await _hubConnection!.InvokeAsync("AtualizarProcessosDash", novos4Processos, cancellationToken);
        }
        novoObservadorAdicionado = false;
    }
    private async Task EnviarProcessosDetalhados(List<ProcessoInfo> top20Processos, CancellationToken cancellationToken)
    {
        await _hubConnection!.InvokeAsync("AtualizarProcessosDetalhados", top20Processos, cancellationToken);
    }

    private (double modoUsuario, double modoPrivilegiado, double usoTotal) CalcularUsoCpu(Process processo)
    {
        try
        {

            var tempoAtual = DateTime.UtcNow;
            var tempoAtualCpuModoUsuario = processo.UserProcessorTime;
            var tempoAtualCpuModoPrivilegiado = processo.PrivilegedProcessorTime;
            var tempoAtualTotalCpu = processo.TotalProcessorTime;

            if (!_ultimoTempoLido.TryGetValue(processo.Id, out var tempoAnterior))
            {
                _ultimoTempoLido[processo.Id] = tempoAtual;
                _tempoCpuModoUsuario[processo.Id] = tempoAtualCpuModoUsuario;
                _tempoCpuModoPrivilegiado[processo.Id] = tempoAtualCpuModoPrivilegiado;
                _tempoCpuTotal[processo.Id] = tempoAtualTotalCpu;

                return (0, 0, 0);
            }

            var tempoDecorrido = 0.5;

            var cpuModoUsuarioTempoDecorrido = (tempoAtualCpuModoUsuario - _tempoCpuModoUsuario[processo.Id]).TotalSeconds;

            var cpuModoPrivilegiadoTempoDecorrido = (tempoAtualCpuModoPrivilegiado - _tempoCpuModoPrivilegiado[processo.Id]).TotalSeconds;

            var cpuTotalTempoDecorrido = (tempoAtualTotalCpu - _tempoCpuTotal[processo.Id]).TotalSeconds;

            _tempoCpuModoPrivilegiado[processo.Id] = tempoAtualCpuModoPrivilegiado;
            _tempoCpuModoUsuario[processo.Id] = tempoAtualCpuModoUsuario;
            _tempoCpuTotal[processo.Id] = tempoAtualTotalCpu;

            if (cpuTotalTempoDecorrido > 0)
            {
                var usoCpuModoPrivilegiado = Math.Round((cpuModoPrivilegiadoTempoDecorrido / tempoDecorrido) / Environment.ProcessorCount * 100, 2);
                var usoCpuModoUsuario = Math.Round((cpuModoUsuarioTempoDecorrido / tempoDecorrido) / Environment.ProcessorCount * 100, 2);
                var usoCpuTotal = Math.Round((cpuTotalTempoDecorrido / tempoDecorrido) / Environment.ProcessorCount * 100, 2);

                return (usoCpuModoPrivilegiado, usoCpuModoUsuario, usoCpuTotal);

            }

            return (0, 0, 0);
        }
        catch (Exception)
        {
            throw new Win32Exception();
        }

    }

    private static void HabilitarAbrirProcessosAoIniciar()
    {
        var processos = Process.GetProcesses().Select(p => new { p.ProcessName, p.MainModule?.FileName }).Where(p => !string.IsNullOrEmpty(p.FileName)).ToList();

        var json = JsonSerializer.Serialize(processos);
        File.WriteAllText(Path.Combine(GerenciadorCredencial.BuscarCaminhoDados(), "resetar_processos.json"), json);
    }

    private void LimparProcessosAntigos(HashSet<int> processosAtuaisIds)
    {
        var processosAnterioresIds = _tempoCpuTotal.Keys.ToList();

        foreach (var id in processosAnterioresIds)
        {
            if (!processosAtuaisIds.Contains(id))
            {
                _tempoCpuTotal.Remove(id);
                _tempoCpuModoUsuario.Remove(id);
                _tempoCpuModoPrivilegiado.Remove(id);
                _ultimoTempoLido.Remove(id);
            }
        }
    }

    private static string RetornarNomeProcesso(Process processo)
    {
        if (processo.MainModule == null)
            return processo.ProcessName;

        FileVersionInfo info = processo.MainModule.FileVersionInfo;

        if (!string.IsNullOrEmpty(processo.MainWindowTitle))
            return processo.MainWindowTitle;
        // A 'FileDescription' é geralmente a mais precisa.
        else if (!string.IsNullOrEmpty(info.FileDescription))
            return info.FileDescription;
        // Se não houver descrição, o 'ProductName' é uma ótima alternativa.
        else if (!string.IsNullOrEmpty(info.ProductName))
            return info.ProductName;

        return processo.ProcessName;
    }

    private static string? RetornarMaquinaIdUsuarioToken(string? token)
    {
        if (token == null) return string.Empty;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claim = jwt.Claims.FirstOrDefault(c =>
                    c.Type == "maquinaId");

            return claim?.Value ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return string.Empty;
        }
    }

    //private static ProcessoInfo MapearParaProcesso(ProcessoInfoBase processoBase)
    //{
    //    return new ProcessoInfo
    //    {
    //        Id = processoBase.Id,
    //        Nome = processoBase.Nome,
    //        NomeExecutavel = processoBase.NomeExecutavel,
    //        NumeroThreads = processoBase.NumeroThreads,
    //        MemoriaUsoMB = processoBase.MemoriaUsoMB,
    //        MemoriaVirtualUsoMB = processoBase.MemoriaVirtualUsoMB,
    //        UsoCpuModoUsuario = processoBase.UsoCpuModoUsuario,
    //        UsoCpuModoPrivilegiado = processoBase.UsoCpuModoPrivilegiado,
    //        UsoCpuTotal = processoBase.UsoCpuTotal,
    //        SubProcessos = []
    //    };
    //}
}
