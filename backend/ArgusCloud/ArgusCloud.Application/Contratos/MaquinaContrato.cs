namespace ArgusCloud.Application.Contratos
{
    public class MaquinaContrato
    {
        public Guid Id { get; set; }
        public required string Nome { get; set; }
        public required string SistemaOperacional { get; set; }
        public string? LocalizacaoMaquina { get; set; }
    }
}
