using Xunit;
using InterviewTest;
using System.Linq;

namespace Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void CtorCreatesExpectedInstance()
        {
            var sut = new MainViewModel();
            var sensors = sut.Sensors.ToList();
            Assert.Equal(2, sensors.Count);
            Assert.Equal("Sensor 1", sensors[0].Name);
            Assert.Equal("Sensor 2", sensors[1].Name);
        }
    }
}