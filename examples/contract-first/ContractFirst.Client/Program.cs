using ContractFirst.Client;
using Microsoft.AspNetCore.SignalR.Client;

await using var connection = new HubConnectionBuilder()
    .WithUrl("")
    .Build();

Action<string> test = Console.WriteLine;

connection.On("", test);


connection.On<string>("hello", msg => Task.FromResult(5));
connection.OnReceiveMessageAsync(Console.WriteLine);


await connection.SendAsync("", 5, "hi");

//
// on.Test();
//
// on.ReceiveMessageAsync(msg =>
// {
//     Console.WriteLine(msg);
//     return Task.CompletedTask;
// });

//var proxy2 = new ChatHubProxy2(connection);
