namespace Argus.Agent
{
    public class Maquina
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string SistemaOperacional { get; set; }
        public string? LocazacaoMaquina { get; set; }
    }
}
