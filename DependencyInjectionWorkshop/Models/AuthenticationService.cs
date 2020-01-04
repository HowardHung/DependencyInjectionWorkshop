using System;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthenticationService authenticationService, INotification notification)
        {
            _notification = notification;
            _authenticationService = authenticationService;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            if (!isValid) Notify(accountId);

            return isValid;
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId, $"account:{accountId} try to login failed");
        }
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;

        private readonly IProfile _profile;
        //private readonly NotificationDecorator _notificationDecorator;

        public AuthenticationService(IFailedCounter failedCounter, ILogger logger, IOtpService otpService,
            IProfile profile, IHash hash, INotification notification)
        {
            _failedCounter = failedCounter;
            _logger = logger;
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
            //_notificationDecorator = new NotificationDecorator(notification);
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _logger = new NLogAdapter();
            //_notificationDecorator = new NotificationDecorator();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            //check account locked
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked) throw new FailedTooManyTimesException {AccountId = accountId};

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(accountId);

                return true;
            }

            //失敗
            _failedCounter.AddFailedCount(accountId);

            LogFailedCount(accountId);

            //_notificationDecorator.Notify(accountId);

            return false;
        }

        private void LogFailedCount(string accountId)
        {
            //紀錄失敗次數 
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}