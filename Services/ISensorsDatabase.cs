using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ISensorsDatabase
    {
        Task<bool> PostAsync(IList<SensorData> data, CancellationToken token);
        Task<IList<SensorData>> GetAllAsync();
        event EventHandler<string> ActivityTracker;
    }
}
