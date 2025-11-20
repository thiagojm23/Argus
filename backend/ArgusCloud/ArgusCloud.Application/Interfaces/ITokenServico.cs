namespace ArgusCloud.Application.Interfaces
{
    public interface ITokenServico
    {
        string GerarTokenTemporarioAgente(string NomeUsuario);
        //string GerarAccessTokenFront(Guid usuarioId, string nomeUsuario);
        //string GerarRefreshTokenFront(Guid usuarioId, string nomeUsuario);
        string GerarTokenDefinitivoAgente(Guid idUsuario, Guid maquinaId);
        string GerarTokenFront(Guid usuarioId, Guid maquinaId);
        bool ValidarClaimsToken(string token, string maquinaId, string tipoCliente);
        bool ValidarToken(string token);
    }
}
