using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public readonly record struct SensorData(string Name, double Value)
    {
        public override string ToString()
        {
            return $"Sensor Data: Name[{Name}], Value[{Value}]";
        }
    }
}
