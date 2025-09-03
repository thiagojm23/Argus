using ArgusCloud.Application.Autenticacao.Requisitos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ArgusCloud.Application.Autenticacao.Handlers
{
    public class MaquinaIdHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<MaquinaIdRequisito>
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MaquinaIdRequisito requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var calimMaquinaId = context.User.FindFirst("maquinaId")?.Value;
            if (calimMaquinaId == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var rotaMaquinaId = httpContext.GetRouteValue("maquinaId")?.ToString();

            var requisicaoMaquinaId = rotaMaquinaId ?? httpContext.Request.Query["maquinaId"].ToString();
            if (string.IsNullOrWhiteSpace(requisicaoMaquinaId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (requisicaoMaquinaId.Equals(rotaMaquinaId)) context.Succeed(requirement);
            else context.Fail();

            return Task.CompletedTask;
        }
    }
}
