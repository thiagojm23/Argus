using System.Collections.Concurrent;
using ArgusCloud.Application.Contratos;
using ArgusCloud.Application.Interfaces;

namespace ArgusCloud.Infrastructure.Servicos
{
    public class InMemoryProcessosTempoRealServico : IProcessoTempoRealServico
    {
        private readonly ConcurrentDictionary<string, IEnumerable<ProcessoContrato>> _estadoProcessos = new();

        public void AtualizarProcessos(string maquinaId, IEnumerable<ProcessoContrato> processos)
        {
            _estadoProcessos[maquinaId] = processos;
        }

        public ProcessoContrato? ObterProcesso(string maquinaId, int idProcesso)
        {
            if (_estadoProcessos.TryGetValue(maquinaId, out var processos))
            {
                return processos.FirstOrDefault(p => p.Id == idProcesso);
            }
            return null;
        }

        public IEnumerable<ProcessoContrato> ObterProcessos(string maquinaId)
        {
            _estadoProcessos.TryGetValue(maquinaId, out var processos);
            return processos ?? [];
        }
    }
}
