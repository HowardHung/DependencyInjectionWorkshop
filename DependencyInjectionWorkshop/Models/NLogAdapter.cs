namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
    {
        private void Info(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}