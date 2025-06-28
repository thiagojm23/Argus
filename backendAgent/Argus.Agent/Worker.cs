using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;


namespace Argus.Agent;

public class Worker(ILogger<Worker> logger, IConfiguration configuracao) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConfiguration _configuracao = configuracao;
    private HubConnection? _hubConnection;
    private readonly string? _tokenAutenticacao;

    private readonly Dictionary<int, TimeSpan> _tempoCpuModoUsuario = [];
    private readonly Dictionary<int, TimeSpan> _tempoCpuModoPrivilegiado = [];
    private readonly Dictionary<int, TimeSpan> _tempoCpuTotal = [];
    private readonly Dictionary<int, DateTime> _ultimoTempoLido = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private List<ProcessoInfo> BuscarTop20ProcessosSistema()
    {
        var top20Processos = Process.GetProcesses().OrderByDescending(p => p.WorkingSet64).Take(20).ToList();
        var processosRetorno = new List<ProcessoInfo>();
        var processosAtuais = Process.GetProcesses();


        foreach (var processo in processosAtuais)
        {
            var (usoCpuModoUsuario, usoCpuModoPrivilegiado, usoCpuTotal) = CalcularUsoCpu(processo);
            try
            {
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
            }
            catch
            {

            }
        }

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
}
