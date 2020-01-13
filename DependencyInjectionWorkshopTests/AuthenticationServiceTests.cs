using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_Valid()
        {
            var profile = Substitute.For<IProfile>();
            var logger = Substitute.For<ILogger>();
            var hash = Substitute.For<IHash>();
            var failCounter = Substitute.For<IFailCounter>();
            var notification = Substitute.For<INotification>();
            var otpService = Substitute.For<IOtpService>();
            var authenticationService =
                new AuthenticationService(profile, hash, otpService, notification, failCounter, logger);
            profile.GetPassword("joey").Returns("my hashed password");
            hash.Compute("1234").Returns("my hashed password");
            otpService.GetCurrentOtp("joey").Returns("123456");
            var isValid = authenticationService.Verify("joey", "1234", "123456");
            Assert.IsTrue(isValid);
        }
    }
}