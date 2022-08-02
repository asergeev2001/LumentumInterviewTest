using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Services
{
    public class SensorDataBuffer : ISensorDataBuffer
    {
        private readonly BufferBlock<IList<SensorData>> _buffer = new BufferBlock<IList<SensorData>>();

        public async Task<IList<SensorData>> ReceiveAsync(CancellationToken token)
        {
            return await _buffer.ReceiveAsync(token);
        }

        public bool Post(IList<SensorData> data)
        {
            return _buffer.Post(data);
        }
    }
}
