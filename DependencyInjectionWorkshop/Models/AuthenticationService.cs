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
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailCounter _failCounter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService(IProfile profile, IHash hash, OtpService otpService, SlackAdapter slackAdapter, FailCounter failCounter, NLogAdapter nLogAdapter)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _slackAdapter = slackAdapter;
            _failCounter = failCounter;
            _nLogAdapter = nLogAdapter;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _failCounter = new FailCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            //check lock state
            var isLocked = _failCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
            var passwordFromDb = _profile.GetPasswordFromDb(accountId);

            var hashedPassword = _hash.GetHashedPassword(password);
            var currentOtp = _otpService.GetCurrentOtp(accountId);
            if (passwordFromDb==hashedPassword&&currentOtp == otp)
            {
                _failCounter.ResetFailedCount(accountId);
                return true;
            }
            else
            {
                //failed
                _failCounter.AddFailedCount(accountId);
                //log
                LogFailCount(accountId);

                _slackAdapter.Notify(accountId);
                return false;
            }
        }

        private void LogFailCount(string accountId)
        {
            var failedCount = GetFailedCount(accountId);
            _nLogAdapter.Info( $"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailedCount(string accountId)
        {
            var failedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}