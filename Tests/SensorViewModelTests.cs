using Xunit;
using InterviewTest;
using System.Linq;
using Moq;
using Services;
using System.Threading.Tasks;

namespace Tests
{
    public class SensorViewModelTests
    {
        private const string SensorName = "Sensor 1";
        private const double SensorValue = 23.4;
        private readonly SensorData _data = new(SensorName, SensorValue);
        private readonly Mock<ISensorDataManager> _dataManager;
        private readonly SensorViewModel _sut;

        public SensorViewModelTests()
        {
            _dataManager = new Mock<ISensorDataManager>(MockBehavior.Strict);
            _sut = new SensorViewModel(SensorName, _dataManager.Object);
            Assert.Equal(SensorName, _sut.Name);
            Assert.False(_sut.PostValue.CanExecute(null));
        }

        [Fact]
        public void LogValueCommandInvokesISensorDataManagerPost()
        {
            _sut.Value = SensorValue.ToString();
            Assert.True(_sut.PostValue.CanExecute(null));
            _dataManager
                .Setup(c => c.Post(_data))
                .Verifiable();
            _sut.PostValue.Execute(null);
            _dataManager.VerifyAll();
        }
    }
}
