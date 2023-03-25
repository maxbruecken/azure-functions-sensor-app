using System;
using System.Threading.Tasks;
using AzureFunction.Core.DbContext;
using AzureFunction.Core.Entities;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Mappers;
using AzureFunction.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureFunction.Core.Repositories;

public class SensorAlarmRepository : ISensorAlarmRepository
{
    private readonly SensorAppContext _context;

    public SensorAlarmRepository(SensorAppContext context)
    {
        _context = context;
    }

    public async Task InsertAsync(SensorAlarm alarm)
    {
        var entity = SensorAlarmMapper.Map(alarm);
        var sensorEntity = await _context.Set<SensorEntity>().SingleOrDefaultAsync(x => x.BoxId == entity.Sensor.BoxId && x.Type == entity.Sensor.Type)
            ?? throw new InvalidOperationException("Sensor not found");
        entity.Sensor = sensorEntity;
        await _context.Set<SensorAlarmEntity>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SensorAlarm alarm)
    {
        var existingEntity = await GetExistingEntity(alarm.Sensor.BoxId, alarm.Sensor.Type, alarm.Status) ?? throw new InvalidOperationException("Sensor alarm not found");
        SensorAlarmMapper.Update(alarm, existingEntity);
        _context.Set<SensorAlarmEntity>().Update(existingEntity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SensorAlarm alarm)
    {
        var existingEntity = await GetExistingEntity(alarm.Sensor.BoxId, alarm.Sensor.Type, alarm.Status);
        if (existingEntity is null)
        {
            return;
        }

        _context.Set<SensorAlarmEntity>().Remove(existingEntity);
        await _context.SaveChangesAsync();
    }

    private async Task<SensorAlarmEntity?> GetExistingEntity(string sensorBoxId, SensorType sensorType, SensorAlarmStatus status)
    {
        var entity = await _context.Set<SensorAlarmEntity>()
            .Include(x => x.Sensor)
            .FirstOrDefaultAsync(x => x.Sensor.BoxId == sensorBoxId && x.Sensor.Type == sensorType && x.Status == status);
        return entity;
    }
}