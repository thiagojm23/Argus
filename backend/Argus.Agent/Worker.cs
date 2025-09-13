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
    public readonly string? _maquinaId = RetornarMaquinaIdUsuarioToken(GerenciadorCredencial.LerToken());
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
                    var top20Processos = BuscarTop20ProcessosSistema();
                    _logger.LogInformation("Enviando os {count} processos que mais consomem memória.", top20Processos.Count);
                    await _hubConnection.InvokeAsync("AtualizarProcessos", _maquinaId, top20Processos, stoppingToken);
                    var caminhoArquivo = Path.Combine(GerenciadorCredencial.BuscarCaminhoDados(), "resetar_processos.json");
                    if (File.Exists(caminhoArquivo)) HabilitarAbrirProcessosAoIniciar();
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
        var hubUri = _configuracao["Backend:SignalRHubUrl"];
        var apiBaseUri = configuracao["Backend:ApiBaseUrl"];

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
        var processosRetorno = new List<ProcessoInfo>();
        var processosAtuaisIds = new HashSet<int>();
        var executaveis = new List<string>();

        var contador = 0;
        while (processosRetorno.Count < 20)
        {
            try
            {
                var (usoCpuModoUsuario, usoCpuModoPrivilegiado, usoCpuTotal) = CalcularUsoCpu(topProcessos[contador]);
                var processoAtual = new ProcessoInfoBase
                {
                    Id = topProcessos[contador].Id,
                    Nome = RetornarNomeProcesso(topProcessos[contador]),
                    NomeExecutavel = topProcessos[contador].MainModule?.FileName ?? "desconhecido",
                    NumeroThreads = topProcessos[contador].Threads.Count,
                    MemoriaUsoMB = topProcessos[contador].WorkingSet64 / (1024 * 1024),
                    MemoriaVirtualUsoMB = topProcessos[contador].VirtualMemorySize64 / (1024 * 1024),
                    UsoCpuModoUsuario = usoCpuModoUsuario,
                    UsoCpuModoPrivilegiado = usoCpuModoPrivilegiado,
                    UsoCpuTotal = usoCpuTotal
                };
                if (!executaveis.Contains(processoAtual.NomeExecutavel))
                {
                    executaveis.Add(processoAtual.NomeExecutavel);
                    processosAtuaisIds.Add(processoAtual.Id);
                    processosRetorno.Add(MapearParaProcesso(processoAtual));
                }
                else
                {
                    ProcessoInfo processoJaListado = processosRetorno.Find(processo => processo.NomeExecutavel == processoAtual.NomeExecutavel)!;
                    processoJaListado.SubProcessos.Add(processoAtual);
                }
            }
            catch (Win32Exception ex)
            {
                continue;
            }
            finally
            {
                contador++;
            }
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

    private static ProcessoInfo MapearParaProcesso(ProcessoInfoBase processoBase)
    {
        return new ProcessoInfo
        {
            Id = processoBase.Id,
            Nome = processoBase.Nome,
            NomeExecutavel = processoBase.NomeExecutavel,
            NumeroThreads = processoBase.NumeroThreads,
            MemoriaUsoMB = processoBase.MemoriaUsoMB,
            MemoriaVirtualUsoMB = processoBase.MemoriaVirtualUsoMB,
            UsoCpuModoUsuario = processoBase.UsoCpuModoUsuario,
            UsoCpuModoPrivilegiado = processoBase.UsoCpuModoPrivilegiado,
            UsoCpuTotal = processoBase.UsoCpuTotal,
            SubProcessos = []
        };
    }
}
