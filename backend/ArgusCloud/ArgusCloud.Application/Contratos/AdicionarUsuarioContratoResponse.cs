namespace ArgusCloud.Application.Contratos
{
    public class AdicionarUsuarioContratoResponse
    {
        public required string Nome { get; set; }
        public required string Senha { get; set; }
        public required string TokenAgente { get; set; }

    }
}
