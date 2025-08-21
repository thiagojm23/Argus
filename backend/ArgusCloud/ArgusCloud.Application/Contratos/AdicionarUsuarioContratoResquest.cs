using System.ComponentModel.DataAnnotations;

namespace ArgusCloud.Application.Contratos
{
    public class AdicionarUsuarioContratoResquest
    {
        private string? _nome;
        private string? _senha;

        [Required(ErrorMessage = "Nome é obrigatório", AllowEmptyStrings = false)]
        public required string Nome
        {
            get => _nome!;
            set => _nome = value?.Trim();
        }
        [Required(ErrorMessage = "Senha é obrigatório", AllowEmptyStrings = false)]
        public required string Senha
        {
            get => _senha!;
            set => _senha = value?.Trim();
        }
        public string? TokenTemporario { get; set; }
    }
}
