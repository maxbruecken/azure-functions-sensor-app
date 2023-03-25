using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AzureFunction.Core.DbContext;
using AzureFunction.Core.Entities;
using AzureFunction.Core.Interfaces;
using AzureFunction.Core.Mappers;
using AzureFunction.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureFunction.Core.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly SensorAppContext _context;

    public SensorRepository(SensorAppContext context)
    {
        _context = context;
    }

    public async Task<Sensor?> GetByBoxIdAndTypeAsync(string boxId, SensorType type, bool includeAlarms = false)
    {
        var entity = await TryGetExistingEntityEntity(boxId, type, includeAlarms);
        return SensorMapper.Map(entity);
    }

    public async Task<IReadOnlyCollection<Sensor>> GetAllAsync(bool includeAlarms = false)
    {
        IQueryable<SensorEntity> query = _context.Set<SensorEntity>();
        if (includeAlarms)
        {
            query = query.Include(x => x.Alarms);
        }
        var entities = await query.ToListAsync();
        return entities.Select(e => SensorMapper.Map(e)!).ToImmutableList();
    }

    public async Task InsertAsync(Sensor sensor)
    {
        var entity = SensorMapper.Map(sensor);
        await _context.Set<SensorEntity>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Sensor sensor)
    {
        var existingEntity = await TryGetExistingEntityEntity(sensor.BoxId, sensor.Type) ?? throw new InvalidOperationException("Sensor not found");
        SensorMapper.Update(sensor, existingEntity);
        _context.Set<SensorEntity>().Update(existingEntity);
        await _context.SaveChangesAsync();
    }

    private async Task<SensorEntity?> TryGetExistingEntityEntity(string boxId, SensorType type, bool includeAlarms = false)
    {
        IQueryable<SensorEntity> query = _context.Set<SensorEntity>();
        if (includeAlarms)
        {
            query = query.Include(x => x.Alarms);
        }
        return await query.SingleOrDefaultAsync(x => x.BoxId == boxId && x.Type == type);
    }
}