using Microsoft.AspNetCore.SignalR;

namespace ArgusCloud.API.Hubs
{
    public class MaquinaIdSignalR : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext context)
        {
            return context.GetHttpContext()!.Request.Query["maquinaId"].ToString();
        }
    }
}
