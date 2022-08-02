using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Services;

namespace InterviewTest
{
    public class MainViewModel : ObservableObject
    {
        private readonly ISensorDataManager _dataManager;
        public MainViewModel()
            : this(new SensorDataManager())
        {

        }
        public MainViewModel(ISensorDataManager dataManager)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _dataManager.ActivityTracker += OnDataPosted;
            Sensors = new List<SensorViewModel>
            { 
                new SensorViewModel("Sensor 1", _dataManager),
                new SensorViewModel("Sensor 2", _dataManager)
            };
            _data = new();
            Data = new(_data);
            _dbMessage = String.Empty;
        }

        private async void OnDataPosted(object? sender, string e)
        {
            await App.Current.Dispatcher.Invoke(async () =>
            {
                DbMessage = e;
                _data.Clear();
                var data = await _dataManager.QueryAllDataAsync();
                foreach (var dt in data) _data.Add(dt);
            });
        }

        public IEnumerable<SensorViewModel> Sensors { get; }
        private ObservableCollection<SensorData> _data;
        public ReadOnlyObservableCollection<SensorData> Data { get; }

        private string _dbMessage;
        public string DbMessage
        {
            get { return _dbMessage; }
            set { SetProperty(ref _dbMessage, value); }
        }
    }
}
