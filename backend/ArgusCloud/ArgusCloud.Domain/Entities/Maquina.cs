namespace Argus.Agent
{
    public class Maquina
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public required string Nome { get; set; }
        public required string SistemaOperacional { get; set; }
        public string? LocalizacaoMaquina { get; set; }
    }
}
