using System.Threading.Tasks;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Interfaces;


public interface ISensorAlarmRepository
{
    Task InsertAsync(SensorAlarm alarm);
    Task UpdateAsync(SensorAlarm alarm);
    Task DeleteAsync(SensorAlarm alarm);
}