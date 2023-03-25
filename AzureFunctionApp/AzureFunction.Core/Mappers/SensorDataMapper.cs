using AzureFunction.Core.Entities;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Mappers;

public static class SensorDataMapper
{
    public static SensorDataEntity Map(AggregatedSensorData model)
    {
        return new SensorDataEntity
        {
            Sensor = SensorMapper.Map(model.Sensor),
            AggregationType = model.AggregationType,
            CreatedAt = model.CreatedAt,
            Value = model.Value
        };
    }
}