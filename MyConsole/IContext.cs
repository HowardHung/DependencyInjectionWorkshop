namespace MyConsole
{
    public interface IContext
    {
        User GetUser();
        void SetUser(string userName);
    }
}