using ArgusCloud.Application.Interfaces;
using MediatR;

namespace ArgusCloud.Application.Comandos
{
    public class AtualizarProcessosEmMemoriaHandler(IProcessoTempoRealServico processoServico) : IRequestHandler<AtualizarProcessosEmMemoriaComando>
    {
        private readonly IProcessoTempoRealServico _processoTempoRealServico = processoServico;

        public Task Handle(AtualizarProcessosEmMemoriaComando request, CancellationToken cancellationToken)
        {
            _processoTempoRealServico.AtualizarProcessos(request.MaquinaId, request.Processos);

            return Task.CompletedTask;
        }
    }
}
