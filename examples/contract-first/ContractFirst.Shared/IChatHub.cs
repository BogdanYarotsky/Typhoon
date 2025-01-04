namespace ContractFirst.Shared;

public interface IChatHub
{
    Task SendMessageAsync(string message);
}