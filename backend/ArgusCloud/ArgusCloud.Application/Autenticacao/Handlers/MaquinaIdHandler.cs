using ArgusCloud.Application.Autenticacao.Requisitos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace ArgusCloud.Application.Autenticacao.Handlers
{
    public class MaquinaIdHandler(IHttpContextAccessor httpContextAccessor, ILogger<MaquinaIdHandler> logger) : AuthorizationHandler<MaquinaIdRequisito>
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger _logger = logger;
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MaquinaIdRequisito requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var claimMaquinaId = context.User.FindFirst("maquinaId")?.Value;
            if (claimMaquinaId == null)
            {
                _logger.LogError("Falha ao acessar claim maquinaId");
                context.Fail();
                return Task.CompletedTask;
            }

            var rotaMaquinaId = httpContext.GetRouteValue("maquinaId")?.ToString();

            var requisicaoMaquinaId = rotaMaquinaId ?? httpContext.Request.Query["maquinaId"].ToString();
            if (string.IsNullOrWhiteSpace(requisicaoMaquinaId))
            {
                _logger.LogError("Erro ao pegar maquinadId da requisição");
                context.Fail();
                return Task.CompletedTask;
            }

            if (requisicaoMaquinaId.Equals(claimMaquinaId))
            {
                _logger.LogInformation("Caindo --------------");
                context.Succeed(requirement);
            }
            else context.Fail();

            return Task.CompletedTask;
        }
    }
}
