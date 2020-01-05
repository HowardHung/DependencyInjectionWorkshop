﻿namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (isValid)
                Reset(accountId);
            else
                AddFailCount(accountId);

            return isValid;
        }

        private void Reset(string accountId)
        {
            _failedCounter.Reset(accountId);
        }

        private void AddFailCount(string accountId)
        {
            _failedCounter.AddFailedCount(accountId);
        }
    }
}