using System.Collections.Concurrent;
using System.Text.Json;
using ArgusCloud.Application.Interfaces;

namespace ArgusCloud.Application.Servicos
{
    public class GerenciadorGruposServico : IGerenciadorGruposServico
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _grupos = new();
        public void AdicionarNoGrupo(string idConexao, string nomeGrupo)
        {
            _grupos.GetOrAdd(nomeGrupo.Trim(), _ => []).TryAdd(idConexao, 0);
        }

        public IEnumerable<string> RemoverDeTodosOsGrupos(string idConexao)
        {
            List<string> nomeGrupos = [];

            foreach (var agente in _grupos)
                if (agente.Value.TryRemove(idConexao, out _))
                    nomeGrupos.Add(agente.Key);

            return nomeGrupos;
        }

        public void RemoverDoGrupo(string idConexao, string nomeGrupo)
        {
            if (_grupos.TryGetValue(nomeGrupo.Trim(), out var grupo))
                grupo.TryRemove(idConexao, out _);
            else
                Console.WriteLine($"Nenhuma conexão com este id {idConexao} para remover deste grupo {nomeGrupo}");
        }

        public int QtdObservadoresAgente(string nomeGrupo)
        {
            Console.WriteLine(JsonSerializer.Serialize(_grupos));
            if (_grupos.TryGetValue(nomeGrupo.Trim(), out var grupo))
            {
                Console.WriteLine(JsonSerializer.Serialize(grupo));
                Console.WriteLine($"Conta {grupo.Count}");
                return grupo.Count;
            }
            return 0;
        }
    }
}
