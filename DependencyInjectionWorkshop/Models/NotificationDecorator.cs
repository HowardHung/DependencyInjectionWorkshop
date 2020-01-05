namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationDecoratorBase : IAuthentication
    {
        private readonly IAuthentication _authentication;

        public AuthenticationDecoratorBase(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            return _authentication.Verify(accountId, password, otp);
        }
    }

    public class NotificationDecorator : AuthenticationDecoratorBase
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(
            authentication)
        {
            _notification = notification;
            _authentication = authentication;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (!isValid) Notify(accountId);

            return isValid;
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId, $"account:{accountId} try to login failed");
        }
    }
}