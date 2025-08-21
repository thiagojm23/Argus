using ArgusCloud.Application.Contratos;
using ArgusCloud.Domain.Entities;
using MediatR;

namespace ArgusCloud.Application.Comandos
{
    public sealed record CadastrarComando(string NomeUsuario, string Senha) : IRequest<AdicionarUsuarioContratoResponse>;
    public sealed record GerarTokenDefinitivoComando(Usuario Usuario, string Senha) : IRequest<AdicionarUsuarioContratoResponse>;
    public sealed record ValidarClaimsTokenComando(string Token, string MaquinaId, string TipoCliente) : IRequest<bool>;
}
