using AzureFunction.Core.Entities;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Mappers;

public static class SensorAlarmMapper
{
    public static SensorAlarm Map(SensorAlarmEntity entity, SensorEntity sensorEntity)
    {
        var sensor = SensorMapper.Map(sensorEntity, true);
        return new SensorAlarm(sensor, entity.Identifier)
        {
            Status = entity.Status,
            FiredAt = entity.FiredAt
        };
    }

    public static SensorAlarmEntity Map(SensorAlarm model)
    {
        return new SensorAlarmEntity
        {
            Sensor = SensorMapper.Map(model.Sensor),
            Identifier = model.Identifier,
            Status = model.Status,
            FiredAt = model.FiredAt
        };
    }

    public static void Update(SensorAlarm model, SensorAlarmEntity existingEntity)
    {
        existingEntity.Status = model.Status;
        existingEntity.FiredAt = model.FiredAt;
    }
}