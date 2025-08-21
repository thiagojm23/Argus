namespace ArgusCloud.Application.Contratos
{
    public class UsuarioContrato
    {
        public Guid IdUsuario { get; set; }
        public required string Nome { get; set; }
        public DateTime DataExpiracao { get; set; }
        public bool ExporProcessos { get; set; }
        public MaquinaContrato? Maquina { get; set; }
        public bool PermiteEspelharemProcessos { get; set; }
        public string? Token { get; set; }
    }
}
