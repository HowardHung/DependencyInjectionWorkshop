using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();

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
            var failedCount = GetFailedCount(accountId);
            _nLogAdapter.Info( $"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailedCount(string accountId)
        {
            //紀錄失敗次數 
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}