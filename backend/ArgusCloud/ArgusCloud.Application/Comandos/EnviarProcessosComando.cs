using ArgusCloud.Application.Contratos;
using MediatR;

namespace ArgusCloud.Application.Comandos
{
    public record EnviarProcessosComando(string MaquinaId, IEnumerable<ProcessoContrato> Processos) : IRequest;
    //public class EnviarMensagemComando(string Conteudo, string IdUsuario)
    //{
    //    public string conteudo { get; } = Conteudo;
    //    public string idUsuario { get; } = IdUsuario;
    //}
}
