using ContractFirst.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();
app.MapHub<ChatHub>("/");
app.Run();