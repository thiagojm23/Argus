using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;


namespace Argus.Agent;

public class Worker(ILogger<Worker> logger, IConfiguration configuracao) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConfiguration _configuracao = configuracao;
    private HubConnection? _hubConnection;
    private readonly string? _tokenAutenticacao = GerenciadorCredencial.LerToken();

    private readonly Dictionary<int, TimeSpan> _tempoCpuModoUsuario = [];
    private readonly Dictionary<int, TimeSpan> _tempoCpuModoPrivilegiado = [];
    private readonly Dictionary<int, TimeSpan> _tempoCpuTotal = [];
    private readonly Dictionary<int, DateTime> _ultimoTempoLido = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_tokenAutenticacao))
        {
            _logger.LogError("Token de autenticação não encontrado. Execute o agente com o parâmetro --register para configurar.");
            return;
        }

        await ConfigurarSignalR(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    var top20Processos = BuscarTop20ProcessosSistema();
                    _logger.LogInformation("Enviando os {count} processos que mais consomem memória.", top20Processos.Count);
                    await _hubConnection.InvokeAsync("SendProcessData", top20Processos, stoppingToken);
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
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ConfigurarSignalR(CancellationToken tokenCancelamento)
    {
        var hubUrl = _configuracao["Backend:SignalRHubUrl"];
        _hubConnection = new HubConnectionBuilder().WithUrl(hubUrl!, options =>
        {
            options.AccessTokenProvider = () => Task.FromResult(_tokenAutenticacao);
        }).WithAutomaticReconnect().Build();

        _hubConnection.On<bool>("RestaurarSessaoAoIniciar", (isEnabled) =>
        {
            _logger.LogInformation("Recebido comando para {status} a funcionalidade de reiniciar apps.", isEnabled ? "habilitar" : "desabilitar");

            if (isEnabled)
                SalvarProcessosAtuaisReinicio();
            else
                LimparProcessosSalvos();
        });

        try
        {
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
        var top20Processos = Process.GetProcesses().OrderByDescending(p => p.WorkingSet64).Take(20).ToList();
        var processosRetorno = new List<ProcessoInfo>();
        var processosAtuaisIds = new HashSet<int>();


        foreach (var processo in top20Processos)
        {
            var (usoCpuModoUsuario, usoCpuModoPrivilegiado, usoCpuTotal) = CalcularUsoCpu(processo);
            processosRetorno.Add(new ProcessoInfo
            {
                Id = processo.Id,
                Nome = processo.ProcessName,
                NumeroThreads = processo.Threads.Count,
                MemoriaUsoMB = processo.WorkingSet64 / (1024 * 1024),
                MemoriaVirtualUsoMB = processo.VirtualMemorySize64 / (1024 * 1024),
                UsoCpuModoUsuario = usoCpuModoUsuario,
                UsoCpuModoPrivilegiado = usoCpuModoPrivilegiado,
                UsoCpuTotal = usoCpuTotal
            });
            processosAtuaisIds.Add(processo.Id);
        }

        LimparProcessosAntigos(processosAtuaisIds);

        return processosRetorno;
    }

    private (double modoUsuario, double modoPrivilegiado, double usoTotal) CalcularUsoCpu(Process processo)
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

        var tempoDecorrido = (tempoAtual - tempoAnterior).TotalSeconds;
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

    private static void SalvarProcessosAtuaisReinicio()
    {
        var processos = Process.GetProcesses().Select(p => new { p.ProcessName, p.MainModule?.FileName }).Where(p => !string.IsNullOrEmpty(p.FileName)).ToList();

        var json = JsonSerializer.Serialize(processos);
        File.WriteAllText(Path.Combine(GerenciadorCredencial.BuscarCaminhoDados(), "resetar_processos.json"), json);
    }

    private static void LimparProcessosSalvos()
    {
        var caminhoArquivo = Path.Combine(GerenciadorCredencial.BuscarCaminhoDados(), "resetar_processos.json");
        if (File.Exists(caminhoArquivo))
            File.Delete(caminhoArquivo);
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
}
