using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Wrapper over BufferBlock to make extension methods mockable
    /// </summary>
    public interface ISensorDataBuffer
    {
        bool Post(IList<SensorData> data);
        Task<IList<SensorData>> ReceiveAsync(CancellationToken token);
    }
}
