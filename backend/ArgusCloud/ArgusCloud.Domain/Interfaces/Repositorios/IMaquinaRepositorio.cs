using Argus.Agent;

namespace ArgusCloud.Domain.Interfaces.Repositorios
{
    public interface IMaquinaRepositorio
    {
        Task<Maquina> CadastrarMaquinaAsync(Maquina Maquina);
        Task<Maquina?> ObterPorIdAsync(Guid id);
        Task<Maquina?> ObterPorNomeAsync(string nome);
        Task<Maquina?> ObterPorMaquinaIdAsync(Guid maquinaId);
        Task AtualizarAsync(Maquina Maquina);
    }
}
