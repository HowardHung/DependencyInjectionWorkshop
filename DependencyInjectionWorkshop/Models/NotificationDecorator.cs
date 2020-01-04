namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecoratorBase : IAuthenticationService
    {
        protected IAuthenticationService _authenticationService;

        public NotificationDecoratorBase(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }
    }

    public class NotificationDecorator : NotificationDecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthenticationService authenticationService, INotification notification) : base(authenticationService)
        {
            _notification = notification;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid) Notify(accountId);

            return isValid;
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId, $"account:{accountId} try to login failed");
        }
    }
}