namespace FOG.Commands
{
    interface ICommand
    {
        bool Process(string[] args);
    }
}
