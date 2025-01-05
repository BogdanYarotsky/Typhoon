using ContractFirst.Client;
using Microsoft.AspNetCore.SignalR.Client;

await using var connection = new HubConnectionBuilder()
    .WithUrl("")
    .Build();

var proxy = new ChatHubProxy(connection);
//var proxy2 = new ChatHubProxy2(connection);

proxy.On.ReceiveMessageAsync(msg =>
{ 
    Console.WriteLine(msg);
    return Task.CompletedTask;
});

await proxy.Invoke.SendMessageAsync("Hello World!");
await proxy.Send.SendMessageAsync("Hello World");