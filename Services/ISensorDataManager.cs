
namespace Services
{
    public interface ISensorDataManager
    {
        void Post(SensorData data);
        Task<IList<SensorData>> QueryAllDataAsync();
        event EventHandler<string> ActivityTracker;
    }
}