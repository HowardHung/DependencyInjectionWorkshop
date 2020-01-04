using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();

        public bool Verify(string accountId, string password,string otp)
        {

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLocked = GetAccountIsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId, httpClient);
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                ResetFailedCount(accountId, httpClient);
                return true;
            }
            else
            {
                AddFailedCount(accountId, httpClient);

                LogFailedCount(accountId, httpClient);

                //notify
                Notify(accountId);
                return false;
            }
        }

        private static bool GetAccountIsLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        private static void Notify(string accountId)
        {
            string message = $"account:{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }

        private static void LogFailedCount(string accountId, HttpClient httpClient)
        {
            //紀錄失敗次數 
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static void AddFailedCount(string accountId, HttpClient httpClient)
        {
            //驗證失敗，累計失敗次數
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCount(string accountId, HttpClient httpClient)
        {
            //驗證成功，重設失敗次數
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}