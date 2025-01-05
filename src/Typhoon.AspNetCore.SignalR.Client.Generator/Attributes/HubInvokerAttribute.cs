using System;

namespace Typhoon.AspNetCore.SignalR.Client.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class HubInvokerAttribute<THub> : Attribute where THub : class
{
}