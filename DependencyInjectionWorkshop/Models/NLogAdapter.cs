namespace DependencyInjectionWorkshop.Models
{
    public interface INLogAdapter
    {
        void Info(string message);
    }

    public class NLogAdapter : INLogAdapter
    {
        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}