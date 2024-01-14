namespace Bll.Interfaces;

public interface ICommandFactory
{
    ICommand? CreateCommand(string messageText);
}