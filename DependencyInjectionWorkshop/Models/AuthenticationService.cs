using System;

namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;
        private IAuthentication _authentication;

        public LogDecorator(IAuthentication authentication, ILogger logger, IFailedCounter failedCounter) : base(
            authentication)
        {
            _authentication = authentication;
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid) LogFailedCount(accountId);

            return isValid;
        }

        private void LogFailedCount(string accountId)
        {
            //紀錄失敗次數 
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class AuthenticationService : IAuthentication
    {
        //private readonly FailedCounterDecorator _failedCounterDecorator;
        private readonly IHash _hash;

        private readonly IOtpService _otpService;

        private readonly IProfile _profile;
        //private readonly NotificationDecorator _notificationDecorator;

        public AuthenticationService(IOtpService otpService,
            IProfile profile, IHash hash)
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            //_logDecorator = new LogDecorator(this);
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
            //_notificationDecorator = new NotificationDecorator(notification);
        }

        public AuthenticationService()
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            //_logDecorator = new LogDecorator(this);
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            //_notificationDecorator = new NotificationDecorator();
        }


        public bool Verify(string accountId, string password, string otp)
        {
            //check account locked
            //_failedCounterDecorator.CheckAccountLock(accountId, this);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
                //_failedCounterDecorator.Reset(accountId);

                return true;

            //失敗
            //_failedCounterDecorator.AddFailCount(accountId, this);

            //_logDecorator.LogFailedCount(accountId);

            //_notificationDecorator.Notify(accountId);

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}