namespace ArgusCloud.Application.Interfaces
{
    public interface IGerenciadorGruposServico
    {
        void RemoverDoGrupo(string idConexao, string nomeGrupo);
        void AdicionarNoGrupo(string idConexao, string nomeGrupo);
        IEnumerable<string> RemoverDeTodosOsGrupos(string idConexao);
        int QtdObservadoresAgente(string nomeGrupo);
    }
}
