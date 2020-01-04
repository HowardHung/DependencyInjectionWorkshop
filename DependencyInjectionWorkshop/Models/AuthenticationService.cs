using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    
    public class AuthenticationService
    {
        private readonly IProfileDao _profileDao;
        private readonly ISha256Adapter _sha256Adapter;
        private readonly IOtpService _otpService;
        private readonly ISlackAdapter _slackAdapter;
        private readonly IFailedCounter _failedCounter;
        private readonly INLogAdapter _nLogAdapter;

        public AuthenticationService(IProfileDao profileDao, ISha256Adapter sha256Adapter, IOtpService otpService, ISlackAdapter slackAdapter, IFailedCounter failedCounter, INLogAdapter nLogAdapter)
        {
            _profileDao = profileDao;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _slackAdapter = slackAdapter;
            _failedCounter = failedCounter;
            _nLogAdapter = nLogAdapter;
        }

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
        }


        public bool Verify(string accountId, string password,string otp)
        {
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.ResetFailedCount(accountId);
                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId);

                LogFailedCount(accountId);

                //notify
                _slackAdapter.Notify(accountId);
                return false;
            }
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _nLogAdapter.Info( $"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}