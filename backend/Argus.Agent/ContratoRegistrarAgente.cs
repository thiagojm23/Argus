namespace Argus.Agent
{
    public class ContratoRegistrarAgente
    {
        public required string NomeUsuario { get; set; }
        public required string Senha { get; set; }
        public required string TokenTemporario { get; set; }
        public required string NomeMaquina { get; set; }
        public string? LocalizacaoMaquina { get; set; }
        public required string SistemaOperacional { get; set; }
    }
}
