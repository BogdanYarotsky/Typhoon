namespace ContractFirst.Shared;

public interface IChatHub
{
    Task SendMessage(string message);
}