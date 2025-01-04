namespace Typhoon.AspNetCore.SignalR.Client.Generator;

[AttributeUsage(AttributeTargets.Class)]
public class HubInvokerAttribute<THub> : Attribute where THub : class
{
}