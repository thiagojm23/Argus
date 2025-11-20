using System.Diagnostics;
using System.Net.Http.Json;
using System.ServiceProcess;
using Argus.Agent;

IHostBuilder builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
});

if (args.Contains("--register"))
{
    await RegistrarAgenteAsync();
    return;
}
if (args.Contains("--uninstall"))
{
    await DesinstalarAgente();
    return;
}

var host = builder.Build();
host.Run();

static async Task RegistrarAgenteAsync()
{
    Console.WriteLine("-- Registro do agente --");

    Console.WriteLine("Digite seu nome de usuário: ");
    string? nomeUsuario = Console.ReadLine();

    Console.WriteLine("Digite sua senha de acesso");
    string? senha = Console.ReadLine();

    Console.WriteLine("Digite seu token temporário de acesso");
    string? tokenTemporario = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(nomeUsuario) || string.IsNullOrWhiteSpace(tokenTemporario))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usuário, token e senha não podem ser vazios");
        Console.ResetColor();
        return;
    }

    var configuracao = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    var apiBaseUrl = configuracao["Backend:ApiBaseUrl"];

    using var cliente = new HttpClient();
    try
    {
        Console.WriteLine("\nValidando credenciais");

        var contrato = new ContratoRegistrarAgente
        {
            NomeMaquina = Environment.MachineName,
            NomeUsuario = nomeUsuario,
            Senha = senha,
            SistemaOperacional = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            TokenTemporario = tokenTemporario,
        };

        var response = await cliente.PostAsJsonAsync($"{apiBaseUrl}/Usuario/registrarAgente", contrato);

        if (response.IsSuccessStatusCode)
        {
            var authResult = await response.Content.ReadFromJsonAsync<AutenticacaoContrato>();
            if (authResult?.TokenAgente is not null)
            {
                GerenciadorCredencial.SalvarToken(authResult.TokenAgente);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nCerto {authResult.NomeUsuario}, seu agente foi registrado com sucesso");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Seu token expira em: {authResult.DataExpiracao:dd/MM/yyyy}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("O serviço agora pode ser iniciado sem passar nenhum argumento\nApenas chamando o <nomeServiço>.exe (Argus.Agent.exe)");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Token inválido");
                Console.ResetColor();
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nFalha na autenticação do token");
            var erro = await response.Content.ReadAsStringAsync();
            Console.WriteLine(erro);
            Console.ResetColor();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nErro ao se conectar com o backend: {ex.Message}");
        Console.ResetColor();
    }
}

static async Task DesinstalarAgente()
{
    Console.WriteLine("Iniciando desinstalação do agente");

    try
    {
        var nomeServico = "Argus.Agent";
        using (var servicoControle = new ServiceController(nomeServico))
        {
            if (servicoControle.Status != ServiceControllerStatus.Stopped)
            {
                Console.WriteLine("Parando serviço");
                servicoControle.Stop();
                servicoControle.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            }
        }

        var psi = new ProcessStartInfo("sc.exe", $"delete \"{nomeServico}\"")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
        };
        Process.Start(psi)!.WaitForExit();
        Console.WriteLine("Serviço removido com sucesso");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Aviso: Não foi possível remover o serviço automaticamente. Erro: {ex.Message}");
        Console.ResetColor();
    }

    try
    {
        var caminhoArquivo = GerenciadorCredencial.BuscarCaminhoDados();
        if (Directory.Exists(caminhoArquivo))
        {
            Directory.Delete(caminhoArquivo, true);
            Console.WriteLine("Dados do agente removidos com sucesso.");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Aviso: Não foi possível deletar os arquivos de dados. Erro: {ex.Message}");
        Console.ResetColor();
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Desinstalação finalizada!");
    Console.ResetColor();

    await Task.CompletedTask;
}
