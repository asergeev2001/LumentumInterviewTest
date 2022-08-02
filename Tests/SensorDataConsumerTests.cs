using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class SensorDataConsumerTests
    {
        private readonly SensorData _data;
        private readonly IList<SensorData> _dataList;
        private readonly SensorDataManager.ISensorDataConsumer _sut;
        private readonly Mock<ISensorDataBuffer> _buffer;
        private readonly Mock<ISensorsDatabase> _db;
        private readonly CancellationTokenSource _tokenSource;

        public SensorDataConsumerTests()
        {
            _data = new("Sensor 1", 23.5);
            _dataList = new List<SensorData> { _data, _data, _data };
            _tokenSource = new();
            _buffer = new Mock<ISensorDataBuffer>(MockBehavior.Strict);
            _db = new Mock<ISensorsDatabase>(MockBehavior.Strict);
            _sut = new SensorDataManager.SensorDataConsumer();
        }

        [Fact]
        public async void ConsumeAsync_Exits_Immediatelly_If_Canceled()
        {
            _tokenSource.Cancel();
            await _sut.ConsumeAsync(_buffer.Object, _db.Object, _tokenSource.Token);
        }

        [Fact]
        public async void ConsumeAsync_Normal_Flow()
        {
            var token = _tokenSource.Token;
            _tokenSource.Cancel();
            MockSequence s = new();
            _buffer.InSequence(s).Setup(b => b.ReceiveAsync(token)).ReturnsAsync(_dataList).Verifiable();
            _db.InSequence(s).Setup(db => db.PostAsync(_dataList, token)).ReturnsAsync(true).Verifiable();
            _buffer.InSequence(s).Setup(b => b.ReceiveAsync(token))
                .ReturnsAsync(() => 
                        {
                            _tokenSource.Cancel();
                            return _dataList;
                        })
                .Verifiable();
            await _sut.ConsumeAsync(_buffer.Object, _db.Object, token);
            _buffer.VerifyAll();
            _db.VerifyAll();
        }
    }
}
