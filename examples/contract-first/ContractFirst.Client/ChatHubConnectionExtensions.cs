using ContractFirst.Shared;
using Typhoon.AspNetCore.SignalR.Client.Generator;

namespace ContractFirst.Client;

[HubInvoker<IChatHub>]
[HubListener<IChatHubClient>]
public static partial class ChatHubConnectionExtensions
{
}