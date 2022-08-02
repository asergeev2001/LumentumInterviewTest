using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    internal class SensorDatabase : ISensorsDatabase
    {
        private readonly List<SensorData> _data = new();
        public event EventHandler<string> ActivityTracker = delegate { };

        public async Task<IList<SensorData>> GetAllAsync()
        {
            return await Task<IList<SensorData>>.Factory.StartNew(() => _data);
        }

        public async Task<bool> PostAsync(IList<SensorData> data, CancellationToken token)
        {

            return await Task<bool>.Factory.StartNew(
                () =>
                {
                    _data.AddRange(data);
                    ActivityTracker?.Invoke(this, $"Sensors DB: Posted {data.Count} new items. Total count[{_data.Count}]");
                    return true;
                }, 
                token);
        }
    }
}
