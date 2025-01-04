using ContractFirst.Shared;
using Microsoft.AspNetCore.SignalR;

namespace ContractFirst.Server;

public class ChatHub : Hub<IChatHubClient>, IChatHub
{
    public Task SendMessageAsync(string message)
    {
        return Clients.Others.ReceiveMessageAsync(message);
    }
}