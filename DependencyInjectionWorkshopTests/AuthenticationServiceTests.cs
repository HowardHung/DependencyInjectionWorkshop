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
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _authenticationService = new AuthenticationService(
                _failedCounter, _logger, _otpService, _profile, _hash, _notification);
        }

        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private ILogger _logger;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private AuthenticationService _authenticationService;

        private void ShouldBeValid()
        {
            var isValid = _authenticationService.Verify("joey", "1234", "123456");

            Assert.IsTrue(isValid);
        }

        private void GivenPasswordFromDb()
        {
            _profile.GetPassword("joey").Returns("my hashed password");
        }

        private void GivenOtp()
        {
            _otpService.GetCurrentOtp("joey").Returns("123456");
        }

        private void GivenHashedPassword()
        {
            _hash.Compute("1234").Returns("my hashed password");
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb();
            GivenHashedPassword();
            GivenOtp();

            ShouldBeValid();
        }
    }
}