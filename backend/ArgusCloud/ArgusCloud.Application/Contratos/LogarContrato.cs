using System.ComponentModel.DataAnnotations;

namespace ArgusCloud.Application.Contratos
{
    public class LogarContrato
    {
        [Required(ErrorMessage = "Nome do usuário obrigatório")]
        public required string NomeUsuario { get; set; }

        [Required(ErrorMessage = "Senha obrigatória")]
        public required string Senha { get; set; }
    }
}
