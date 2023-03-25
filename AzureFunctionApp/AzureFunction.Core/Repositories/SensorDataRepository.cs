using System;
using System.Threading.Tasks;
using AzureFunction.Core.DbContext;
using AzureFunction.Core.Entities;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Mappers;
using AzureFunction.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureFunction.Core.Repositories;

public class SensorDataRepository : ISensorDataRepository
{
    private readonly SensorAppContext _context;
        
    public SensorDataRepository(SensorAppContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(AggregatedSensorData aggregatedSensorData)
    {
        var entity = SensorDataMapper.Map(aggregatedSensorData);
        var sensorEntity = await _context.Set<SensorEntity>().SingleOrDefaultAsync(x => x.BoxId == aggregatedSensorData.Sensor.BoxId && x.Type == aggregatedSensorData.Sensor.Type)
            ?? throw new InvalidOperationException("Sensor not found");
        entity.Sensor = sensorEntity;
        await _context.Set<SensorDataEntity>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}