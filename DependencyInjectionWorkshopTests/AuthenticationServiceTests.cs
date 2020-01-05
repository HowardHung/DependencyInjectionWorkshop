using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();
            _profile = Substitute.For<IProfile>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _authentication =
                new Authentication(_otpService, _profile, _hash);
            _authentication = new FailedCounterDecorator(_authentication, _failedCounter);
            _authentication = new LogDecorator(_authentication, _logger, _failedCounter);
            _authentication = new NotificationDecorator(_authentication, _notification);
        }

        private const string DefaultAccountId = "joey";
        private const int DefaultFailedCount = 91;
        private IAuthentication _authentication;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtpService _otpService;
        private IProfile _profile;

        private void GivenAccountIsLocked(bool isLocked)
        {
            _failedCounter.GetAccountIsLocked(DefaultAccountId).Returns(isLocked);
        }

        private void GivenFailedCount(int failedCount)
        {
            _failedCounter.GetFailedCount(DefaultAccountId).Returns(failedCount);
        }

        private void GivenHashedPassword(string password, string hashedPassword)
        {
            _hash.Compute(password).Returns(hashedPassword);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).Returns(otp);
        }

        private void GivenPasswordFromDb(string accountId, string passwordFromDb)
        {
            _profile.GetPassword(accountId).Returns(passwordFromDb);
        }

        private void LogShouldContains(string accountId, string failedCount)
        {
            _logger.Received(1).Info(
                Arg.Is<string>(message => message.Contains(accountId) && message.Contains(failedCount)));
        }

        private void ShouldAddFailedCount(string accountId)
        {
            _failedCounter.Received(1)
                .AddFailedCount(accountId);
        }

        private void ShouldBeInvalid(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);

            Assert.IsFalse(isValid);
        }

        private void ShouldBeValid(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);

            Assert.IsTrue(isValid);
        }

        private void ShouldNotify(string accountId)
        {
            _notification.Received(1).Notify(accountId, Arg.Any<string>());
        }

        private void ShouldResetFailedCount(string accountId)
        {
            _failedCounter.Received(1)
                .Reset(accountId);
        }

        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => _authentication.Verify(DefaultAccountId, "1234", "123456");
            Assert.Throws<TException>(action);
        }

        private void WhenInvalid()
        {
            GivenPasswordFromDb(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            _authentication.Verify(DefaultAccountId, "1234", "wrong otp");
        }

        private void WhenValid()
        {
            GivenPasswordFromDb(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            _authentication.Verify(DefaultAccountId, "1234", "123456");
        }

        [Test]
        public void account_is_locked()
        {
            GivenAccountIsLocked(true);
            ShouldThrow<FailedTooManyTimesException>();
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount(DefaultAccountId);
        }

        [Test]
        public void is_invalid()
        {
            GivenPasswordFromDb(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            ShouldBeInvalid(DefaultAccountId, "1234", "wrong otp");
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            ShouldBeValid(DefaultAccountId, "1234", "123456");
        }

        [Test]
        public void isValid_Order()
        {
            GivenPasswordFromDb(DefaultAccountId, "my hashed password");
            GivenHashedPassword("1234", "my hashed password");
            GivenOtp(DefaultAccountId, "123456");

            _authentication.Verify(DefaultAccountId, "1234", "wrong otp");

            Received.InOrder(() =>
            {
                _failedCounter.AddFailedCount(DefaultAccountId);
                _logger.Info(Arg.Is<string>(m => m.Contains(DefaultAccountId)));
                _notification.Notify(DefaultAccountId, Arg.Any<string>());
            });
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);
            WhenInvalid();
            LogShouldContains(DefaultAccountId, DefaultFailedCount.ToString());
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotify(DefaultAccountId);
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount(DefaultAccountId);
        }
    }
}