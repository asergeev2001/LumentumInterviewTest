using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace Tests
{

    public class SensorDataManagerTests
    {
        private const int Timeout = 20;
        private const int DataChunkSize = 3;
        private readonly SensorData _data;
        private readonly IList<SensorData> _dataList;
        private readonly Mock<SensorDataManager> _sut;
        private readonly Mock<ISensorDataBuffer> _buffer;
        private readonly Mock<ISensorsDatabase> _db;
        private readonly Mock<SensorDataManager.ISensorDataConsumer> _consumer;


        public SensorDataManagerTests()
        {
            _data = new("Sensor 1", 23.5);
            _dataList = new List<SensorData> { _data, _data, _data };
            _buffer = new Mock<ISensorDataBuffer>(MockBehavior.Strict);
            _db = new Mock<ISensorsDatabase>(MockBehavior.Strict);
            _consumer = new Mock<SensorDataManager.ISensorDataConsumer>(MockBehavior.Strict);
            _sut = new Mock<SensorDataManager>(
                MockBehavior.Strict, _db.Object, DataChunkSize, _buffer.Object, _consumer.Object);
        }

        [Fact]
        public void Post_Posts_Data_To_ISensorDataBuffer_Once()
        {
            _buffer.Setup(b => b.Post(It.IsAny<IList<SensorData>>())).Returns(true);
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _buffer.Verify(b => b.Post(_dataList), Times.Exactly(1));
        }

        [Fact]
        public void Post_Posts_Data_To_ISensorDataBuffer_Twice()
        {
            _buffer.Setup(b => b.Post(It.IsAny<IList<SensorData>>())).Returns(true);
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _buffer.Verify(b => b.Post(_dataList), Times.Exactly(2));
        }

        [Fact]
        public void Post_DoesNot_Post_Data_To_ISensorDataBuffer()
        {
            _sut.Object.Post(_data);
            _sut.Object.Post(_data);
            _buffer.Verify(b => b.Post(_dataList), Times.Exactly(0));
        }

        [Fact]
        public async void Post_With_Productin_ISensorDataBuffer()
        {
            CancellationTokenSource tokenSource = new();
            ISensorDataBuffer buffer = new SensorDataBuffer();
            _consumer.Setup(c => c.ConsumeAsync(buffer, _db.Object, It.IsAny<CancellationToken>())).Verifiable();
            SensorDataManager sut = new(_db.Object, DataChunkSize, buffer, _consumer.Object);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            var receiveTask = buffer.ReceiveAsync(tokenSource.Token);
            Assert.True(receiveTask.Wait(Timeout));
            var data = await receiveTask;
            Assert.NotNull(data);
            Assert.Equal(_dataList, data);
            receiveTask = buffer.ReceiveAsync(tokenSource.Token);
            Assert.True(receiveTask.Wait(Timeout));
            data = await receiveTask;
            Assert.NotNull(data);
            Assert.Equal(_dataList, data);
            sut.Post(_data);
            Assert.False(buffer.ReceiveAsync(tokenSource.Token).Wait(Timeout));
            _consumer.VerifyAll();
        }

        [Fact]
        public async void Post_With_Productin_ISensorDataBuffer_ISensorDataConsumer()
        {
            CancellationTokenSource tokenSource = new();
            ISensorDataBuffer buffer = new SensorDataBuffer();
            SensorDataManager.ISensorDataConsumer consumer = new SensorDataManager.SensorDataConsumer();
            _db.Setup(db => db.PostAsync(_dataList, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            SensorDataManager sut = new(_db.Object, DataChunkSize, buffer, consumer);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            sut.Post(_data);
            Thread.Sleep(1000);
            sut.Dispose();
            _db.Verify(db => db.PostAsync(_dataList, It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
