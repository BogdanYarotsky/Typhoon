using ContractFirst.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Typhoon.AspNetCore.SignalR.Client.Generator.Attributes;

namespace ContractFirst.Client;

[HubSender<IChatHub>]
[HubInvoker<IChatHub>]
[HubListener<IChatHubClient>]
public partial class ChatHubProxy2
{
}

public class ChatHubProxy
{
    // all code below should be source-generated based on the attributes above
    public IListener On { get; }
    public Invoker Invoke { get; }
    public Sender Send { get; }

    public ChatHubProxy(HubConnection connection)
    {
        On = new Listener(connection);
        Send = new(connection);
        Invoke = new(connection);
    }
    
    public class Sender
    {
        private readonly HubConnection _c;
        public Sender(HubConnection c)
        {
            _c = c;
        }
        public Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            return _c.SendCoreAsync(nameof(SendMessageAsync), [message], cancellationToken);
        }
    }
    
    public class Invoker
    {
        private readonly HubConnection _c;
        public Invoker(HubConnection c)
        {
            _c = c;
        }

        public Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            return _c.InvokeCoreAsync(nameof(SendMessageAsync), [message], cancellationToken);
        }
    }
    
    public interface IListener
    {
        IDisposable ReceiveMessageAsync(Func<string, Task> handler);
    }

    private class Listener : IListener
    {
        private readonly HubConnection _c;
        public Listener(HubConnection c)
        {
            _c = c;
        }
        
        public IDisposable ReceiveMessageAsync(Func<string, Task> handler)
        {
            return _c.On(nameof(ReceiveMessageAsync), handler);
        }
    }
}