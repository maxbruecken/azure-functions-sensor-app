using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces
{
    public interface ISensorAlarmRepository
    {
        Task<SensorAlarm> GetBySensorIdAndStatus(string sensorId, AlarmStatus status);
        Task Create(SensorAlarm alarm);
        Task Update(SensorAlarm alarm);
    }
}
