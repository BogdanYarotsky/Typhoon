using System;

namespace Typhoon.AspNetCore.SignalR.Client.Generator.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class HubListenerAttribute<THubClient> : Attribute where THubClient : class
{
}