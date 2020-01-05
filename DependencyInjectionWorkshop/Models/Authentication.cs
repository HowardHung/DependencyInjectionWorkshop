using System;

namespace DependencyInjectionWorkshop.Models
{
    public class Authentication : IAuthenticationService
    {
        private readonly FailedCounterDecorator _failedCounterDecorator;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;

        private readonly IProfile _profile;
        //private readonly NotificationDecorator _notificationDecorator;

        public Authentication(IFailedCounter failedCounter, ILogger logger, IOtpService otpService,
            IProfile profile, IHash hash)
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            FailedCounter = failedCounter;
            _logger = logger;
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
            //_notificationDecorator = new NotificationDecorator(notification);
        }

        public Authentication()
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            FailedCounter = new FailedCounter();
            _logger = new NLogAdapter();
            //_notificationDecorator = new NotificationDecorator();
        }

        public IFailedCounter FailedCounter { get; }

        public bool Verify(string accountId, string password, string otp)
        {
            //check account locked
            var isLocked = FailedCounter.GetAccountIsLocked(accountId);
            if (isLocked) throw new FailedTooManyTimesException {AccountId = accountId};

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
                //_failedCounterDecorator.Reset(accountId);

                return true;

            //失敗
            //_failedCounterDecorator.AddFailCount(accountId, this);

            LogFailedCount(accountId);

            //_notificationDecorator.Notify(accountId);

            return false;
        }

        private void LogFailedCount(string accountId)
        {
            //紀錄失敗次數 
            var failedCount = FailedCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}