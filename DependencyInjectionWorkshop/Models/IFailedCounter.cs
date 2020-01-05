namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void Reset(string accountId);
        void AddFailedCount(string accountId);
        bool GetAccountIsLocked(string accountId);
        [AuditLog]
        int GetFailedCount(string accountId);
    }
}