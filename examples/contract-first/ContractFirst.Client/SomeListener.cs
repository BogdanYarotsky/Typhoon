using ContractFirst.Shared;
using Typhoon.AspNetCore.SignalR.Client.Generator.Attributes;

namespace ContractFirst.Client;

[HubListener<IChatHubClient>]
public partial class SomeListener
{
    
}