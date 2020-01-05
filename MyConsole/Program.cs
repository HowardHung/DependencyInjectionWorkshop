using DependencyInjectionWorkshop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyConsole
{
    class Program
    {

        private static IAuthentication _authentication;
        private static IFailedCounter _failedCounter;
        private static IHash _hash;
        private static ILogger _logger;
        private static INotification _notification;
        private static IOtpService _otpService;
        private static IProfile _profile;

        static void Main(string[] args)
        {
            _otpService = new FakeOtp();
            _hash = new FakeHash();
            _profile = new FakeProfile();
            _logger = new FakeLogger();
            _notification = new FakeSlack();
            _failedCounter = new FakeFailedCounter();
            _authentication =
                new Authentication(_otpService, _profile, _hash);

            _authentication = new FailedCounterDecorator(_authentication, _failedCounter);
            _authentication = new LogDecorator(_authentication, _logger, _failedCounter);
            _authentication = new NotificationDecorator(_authentication, _notification);


            var isValid = _authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
            Console.ReadKey();

        }
    }


    internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"logger: {message}");
        }
    }

    internal class FakeSlack : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Notify(string accountId, string message)
        {
            PushMessage($"{nameof(Notify)}, accountId:{accountId}, message:{message}");
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }

        public void AddFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(AddFailedCount)}({accountId})");
        }

        public bool GetAccountIsLocked(string accountId)
        {
            return IsAccountLocked(accountId);
        }

        public int GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string Compute(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
}
