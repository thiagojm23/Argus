using ArgusCloud.Application.Contratos;

namespace ArgusCloud.Application.Interfaces
{
    public interface IProcessoTempoRealServico
    {
        void AtualizarProcessos(string maquinaId, IEnumerable<ProcessoContrato> processos);
        IEnumerable<ProcessoContrato> ObterProcessos(string maquinaId);
        ProcessoContrato? ObterProcesso(string maquinaId, int idProcesso);
    }
}
