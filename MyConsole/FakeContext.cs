namespace MyConsole
{
    internal class FakeContext : IContext
    {
        private User _user;

        public User GetUser()
        {
            return _user;
        }

        public void SetUser(string userName)
        {
            _user = new User {Name = userName};
        }
    }
}