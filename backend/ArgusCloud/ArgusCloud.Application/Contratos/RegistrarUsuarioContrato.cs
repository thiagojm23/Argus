using System.ComponentModel.DataAnnotations;

namespace ArgusCloud.Application.Contratos
{
    public class RegistrarUsuarioContrato
    {
        [Required(ErrorMessage = "Nome é obrigatório", AllowEmptyStrings = false)]
        public required string NomeUsuario { get; set; }
        [Required(ErrorMessage = "Senha é obrigatório", AllowEmptyStrings = false)]
        public required string Senha { get; set; }
        [Required(ErrorMessage = "Token temporário obrigatório", AllowEmptyStrings = false)]
        public required string TokenTemporario { get; set; }
    }
}
