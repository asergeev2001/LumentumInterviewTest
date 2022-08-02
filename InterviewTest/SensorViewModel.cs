using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InterviewTest
{
    public class SensorViewModel : ObservableObject
    {
        private ISensorDataManager _dataManager;
        public SensorViewModel(string name, ISensorDataManager dataManager)
        {
            Name = name;
            _dataManager = dataManager;
        }

        public string Name { get; }

        private double? _value;
        public string Value
        {
            set
            {
                if (value != null)
                {
                    try
                    {
                        _value = double.Parse(value);
                    }
                    catch (Exception)
                    {
                        _value = null;
                        throw;
                    }
                    finally
                    {
                        PostValue.NotifyCanExecuteChanged();
                    }
                }

            }
        }
        private RelayCommand? _postValue;
        public RelayCommand PostValue
        {
            get
            {
                if (_postValue == null)
                {
                    _postValue = new RelayCommand(
                        () => PostData(),
                        () => _value != null);
                }
                return _postValue;
            }
        }
        private void PostData()
        {
            _dataManager.Post(new SensorData(Name, _value.Value));
        }
    }
}