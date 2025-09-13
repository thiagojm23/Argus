namespace Argus.Agent
{
    public class Usuario
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public DateTime DataExpiracao { get; set; }
        public bool ExporProcessos { get; set; } = true;
        public Guid? MaquinaId { get; set; }
        public virtual Maquina? Maquina { get; set; }
        public bool PermiteEspelharemProcessos { get; set; }
        public required string TokenHash { get; set; }
    }
}
