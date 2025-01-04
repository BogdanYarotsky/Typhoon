namespace Typhoon.AspNetCore.SignalR.Client.Generator;

[AttributeUsage(AttributeTargets.Class)]
public class HubListenerAttribute<THubClient> : Attribute where THubClient : class
{
}