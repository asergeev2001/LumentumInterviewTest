using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Services
{
    /// <summary>
    /// Manages sensors data by keeping them in a cach and flusing
    /// to a database whenever the database sercise is avaialble
    /// 
    /// </summary>
    public class SensorDataManager : ISensorDataManager, IDisposable
    {
        private const int DataChunkSize = 5;

        private readonly IList<SensorData> _data;
        private readonly int _dataChunkSize;
        private readonly ISensorDataBuffer _buffer;
        private readonly CancellationTokenSource _tokenSource;
        private readonly Task _consumerWorker;
        private readonly ISensorDataConsumer _consumer;
        private readonly ISensorsDatabase _db;

        public event EventHandler<string> ActivityTracker = delegate { };

        /// <summary>
        /// Test support constructor. May be declared as protected to be not 
        /// visible by the production code.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dataChunkSize"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public SensorDataManager(
            ISensorsDatabase db,
            int dataChunkSize,
            ISensorDataBuffer buffer,
            ISensorDataConsumer consumer)
        {
            _db = db;
            _db.ActivityTracker += OnDbUpdated;
            _data = new List<SensorData>();
            _dataChunkSize = dataChunkSize;
            _buffer = buffer;
            _tokenSource = new CancellationTokenSource();
            _consumer = consumer;
            _consumerWorker = Task.Factory.StartNew(
                () =>
                    {
                        _consumer.ConsumeAsync(_buffer, _db, _tokenSource.Token);
                    }, 
                TaskCreationOptions.LongRunning);
        }

        private void OnDbUpdated(object? sender, string e)
        {
            ActivityTracker.Invoke(sender, e);
        }

        public SensorDataManager()
            : this(
                  new SensorDatabase(),
                  DataChunkSize,
                  new SensorDataBuffer(),
                  new SensorDataConsumer())
        {
        }

        public void Post(SensorData data)
        {
            lock (_data)
            {
                _data.Add(data);
                if (_data.Count >= _dataChunkSize)
                {
                    _buffer.Post(new List<SensorData>(_data));
                    _data.Clear();
                }
            }
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _consumerWorker.Wait();
        }

        public Task<IList<SensorData>> QueryAllDataAsync()
        {
            return _db.GetAllAsync();
        }

        /// <summary>
        /// Helper interface and class.
        /// Helps to avoid of a private method in the outer class.
        /// </summary>
        public interface ISensorDataConsumer
        {
            Task ConsumeAsync(ISensorDataBuffer buffer, ISensorsDatabase db, CancellationToken token);
        }

        public class SensorDataConsumer : ISensorDataConsumer
        {
            private Queue<IList<SensorData>> _queue = new();
            public async Task ConsumeAsync(
                ISensorDataBuffer buffer,
                ISensorsDatabase db,
                CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    while (_queue.Count > 0 && await db.PostAsync(_queue.Peek(), token))
                    {
                        _queue.Dequeue();
                    }
                    _queue.Enqueue(await buffer.ReceiveAsync(token));
                }
            }
        }
    }
}
