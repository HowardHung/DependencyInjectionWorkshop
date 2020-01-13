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
        private readonly IOtpService _otpService;
        private readonly INotification _notification;
        private readonly IFailCounter _failCounter;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService, INotification notification, IFailCounter failCounter, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _notification = notification;
            _failCounter = failCounter;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _notification = new Notification();
            _failCounter = new FailCounter();
            _logger = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            //check lock state
            var isLocked = _failCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);
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

                _notification.Notify(accountId);
                return false;
            }
        }

        private void LogFailCount(string accountId)
        {
            var failedCount = GetFailedCount(accountId);
            _logger.Info( $"accountId:{accountId} failed times:{failedCount}");
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