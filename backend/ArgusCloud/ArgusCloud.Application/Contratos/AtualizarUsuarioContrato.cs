using System.ComponentModel.DataAnnotations;

namespace ArgusCloud.Application.Contratos
{
    public class AtualizarUsuarioContrato
    {
        [Required(ErrorMessage = "Id obrigatório")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Expor processos obrigatório")]
        public bool ExporProcessos { get; set; }
        [Required(ErrorMessage = "Permite espelharem processos obrigatório")]
        public bool PermiteEspelharemProcessos { get; set; }
    }
}
