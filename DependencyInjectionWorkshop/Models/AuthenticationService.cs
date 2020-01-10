using SlackAPI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailCounter _failCounter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failCounter = new FailCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            //check lock state
            var isLocked = _failCounter.GetAccountIsLocked(accountId, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);
            var currentOtp = _otpService.GetCurrentOtp(accountId, httpClient);
            if (passwordFromDb==hashedPassword&&currentOtp == otp)
            {
                _failCounter.ResetFailedCount(accountId, httpClient);
                return true;
            }
            else
            {
                //failed
                _failCounter.AddFailedCount(accountId, httpClient);
                //log
                LogFailCount(accountId, httpClient);

                _slackAdapter.Notify(accountId);
                return false;
            }
        }

        private void LogFailCount(string accountId, HttpClient httpClient)
        {
            var failedCount = GetFailedCount(accountId, httpClient);
            _nLogAdapter.Info( $"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailedCount(string accountId, HttpClient httpClient)
        {
            var failedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}