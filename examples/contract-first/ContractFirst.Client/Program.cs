using ContractFirst.Client;
using Microsoft.AspNetCore.SignalR.Client;

await using var connection = new HubConnectionBuilder()
    .WithUrl("")
    .Build();

var proxy = new ChatHubProxy(connection);

proxy.On.ReceiveMessage(msg =>
{ 
    Console.WriteLine(msg);
    return Task.CompletedTask;
});

await proxy.Invoke.SendMessage("Hello World!");