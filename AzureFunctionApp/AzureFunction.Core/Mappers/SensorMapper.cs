using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AzureFunction.Core.Entities;
using AzureFunction.Core.Models;

namespace AzureFunction.Core.Mappers;

public static class SensorMapper
{
    [return: NotNullIfNotNull(nameof(entity))]
    public static Sensor? Map(SensorEntity? entity, bool skipAlarms = false)
    {
        return entity is null
            ? null
            : new Sensor(entity.BoxId, entity.Type)
            {
                Information = new SensorInformation(entity.Information.Manufacturer, entity.Information.ModelNumber, entity.Information.SerialNumber),
                LastSeen = entity.LastSeen ?? DateTimeOffset.MinValue,
                Max = entity.Max,
                Min = entity.Min,
                Alarms = skipAlarms ? Array.Empty<SensorAlarm>() : entity.Alarms.Select(x => SensorAlarmMapper.Map(x, entity)).ToImmutableList()
            };
    }

    public static SensorEntity Map(Sensor model)
    {
        return new SensorEntity
        {
            BoxId = model.BoxId,
            Type = model.Type,
            Information = new SensorInformationEntity
            {
                Manufacturer = model.Information.Manufacturer,
                ModelNumber = model.Information.ModelNumber,
                SerialNumber = model.Information.SerialNumber
            },
            LastSeen = model.LastSeen,
            Max = model.Max,
            Min = model.Min
        };
    }

    public static void Update(Sensor model, SensorEntity existingEntity)
    {
        existingEntity.Information = new SensorInformationEntity
        {
            Id = existingEntity.Id,
            Manufacturer = model.Information.Manufacturer,
            ModelNumber = model.Information.ModelNumber,
            SerialNumber = model.Information.SerialNumber
        };
        existingEntity.LastSeen = model.LastSeen;
        existingEntity.Max = model.Max;
        existingEntity.Min = model.Min;
    }
}