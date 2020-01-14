using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces
{
    public interface ISensorAlarmRepository
    {
        Task<SensorAlarm> GetBySensorIdAndStatus(string sensorId, AlarmStatus status);
        Task Insert(SensorAlarm alarm);
        Task Update(SensorAlarm alarm);
        Task Delete(SensorAlarm sensorAlarm);
    }
}
