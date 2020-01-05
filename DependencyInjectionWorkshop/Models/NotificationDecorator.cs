namespace DependencyInjectionWorkshop.Models
{
    
    public class NotificationDecorator : IAuthenticationService
    {
        private readonly INotification _notification;
        private readonly IAuthenticationService _authentication;

        public NotificationDecorator(IAuthenticationService authenticationService, INotification notification)
        {
            _notification = notification;
            _authentication = authenticationService;
        }

        public bool Verify(string accountId, string password, string otp)
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