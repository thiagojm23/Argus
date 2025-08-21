using ArgusCloud.Application.Interfaces;
using MediatR;

namespace ArgusCloud.Application.Comandos
{
    public class EnviarProcessosComandoHandler(IProcessoTempoRealServico processoServico) : IRequestHandler<EnviarProcessosComando>
    {
        private readonly IProcessoTempoRealServico _processoTempoRealServico = processoServico;

        public Task Handle(EnviarProcessosComando request, CancellationToken cancellationToken)
        {
            _processoTempoRealServico.AtualizarProcessos(request.MaquinaId, request.Processos);

            return Task.CompletedTask;
        }
    }
}
