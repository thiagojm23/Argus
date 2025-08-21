using ArgusCloud.Domain.Entities;

namespace ArgusCloud.Domain.Interfaces.Repositorios
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario> CadastrarUsuarioAsync(Usuario usuario);
        Task<Usuario?> ObterPorIdAsync(Guid id);
        Task<Usuario?> ObterPorNomeAsync(string nome);
        Task AtualizarAsync(Usuario usuario);
    }
}
