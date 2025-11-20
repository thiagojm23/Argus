namespace Argus.Agent
{
    public class VerificarAgenteContrato
    {
        public Guid Id { get; set; }
        public required string Nome { get; set; }
        public DateTime DataExpiracao { get; set; }
        public bool ExporProcessos { get; set; }
        public bool PermiteEspelharemProcessos { get; set; }
    }
}
