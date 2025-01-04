using ContractFirst.Shared;
using Microsoft.AspNetCore.SignalR;

namespace ContractFirst.Server;

public class ChatHub : Hub<IChatHubClient>, IChatHub
{
    public Task SendMessage(string message)
    {
        return Clients.Others.ReceiveMessage(message);
    }
}