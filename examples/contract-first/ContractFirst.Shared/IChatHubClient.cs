namespace ContractFirst.Shared;

public interface IChatHubClient
{
    Task ReceiveMessage(string message);
}