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

        public bool Verify(string accountId, string password,string otp)
        {

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLocked = _failedCounter.GetAccountIsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId, httpClient);
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.ResetFailedCount(accountId, httpClient);
                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId, httpClient);

                LogFailedCount(accountId, httpClient);

                //notify
                _slackAdapter.Notify(accountId);
                return false;
            }
        }

        private static void LogFailedCount(string accountId, HttpClient httpClient)
        {
            var failedCount = GetFailedCount(accountId, httpClient);
            Log(accountId, failedCount);
        }

        private static void Log(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailedCount(string accountId, HttpClient httpClient)
        {
            //紀錄失敗次數 
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

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