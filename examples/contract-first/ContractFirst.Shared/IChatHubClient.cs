namespace ContractFirst.Shared;

public interface IChatHubClient
{
    Task ReceiveMessageAsync(string message);
}