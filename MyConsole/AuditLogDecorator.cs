using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    internal class AuditLogDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IContext _context;

        public AuditLogDecorator(IAuthentication authentication, ILogger logger, IContext context) : base(authentication)
        {
            _logger = logger;
            _context = context;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var username = _context.GetUser().Name;
            _logger.Info($"user {username} | parameter {accountId} | {password} | {otp}");
            var isValid = base.Verify(accountId, password, otp);
            _logger.Info($"return value:{isValid}");
            return isValid;
        }
    }
}