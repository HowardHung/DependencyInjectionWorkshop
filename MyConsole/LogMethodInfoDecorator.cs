using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    internal class LogMethodInfoDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            _logger.Info($"parameter {accountId} | {password} | {otp}");
            var isValid = base.Verify(accountId, password, otp);
            _logger.Info($"return value {isValid}");
            return isValid;
        }
    }
}