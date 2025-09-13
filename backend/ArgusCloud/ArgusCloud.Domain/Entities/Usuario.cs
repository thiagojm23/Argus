using Argus.Agent;

namespace ArgusCloud.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public required string Nome { get; set; }
        public DateTime DataExpiracao { get; set; };
        public bool ExporProcessos { get; set; } = true;
        public Guid? MaquinaId { get; set; }
        public virtual Maquina? Maquina { get; set; }
        public bool PermiteEspelharemProcessos { get; set; }
        public required string? TokenAgenteHash { get; set; }
        public required string? TokenTemporarioAgente { get; set; }
        public required string SenhaHash { get; set; }
    }
}
