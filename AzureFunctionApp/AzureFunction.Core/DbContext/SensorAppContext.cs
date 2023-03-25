using AzureFunction.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AzureFunction.Core.DbContext;

public class SensorAppContext : Microsoft.EntityFrameworkCore.DbContext
{
    public SensorAppContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorEntity>(e =>
        {
            e.ToTable("Sensors");
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Information)
                .WithOne()
                .HasForeignKey<SensorInformationEntity>("SensorId")
                .IsRequired();
            e.Navigation(x => x.Information).AutoInclude();
            e.HasIndex(x => new { x.BoxId, x.Type }).IsUnique();
        });
        modelBuilder.Entity<SensorInformationEntity>(e =>
        {
            e.ToTable("SensorInformations");
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasKey(x => x.Id);
        });
        modelBuilder.Entity<SensorAlarmEntity>(e =>
        {
            e.ToTable("SensorAlarms");
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Sensor)
                .WithMany(x => x.Alarms)
                .HasPrincipalKey(x => x.Id)
                .HasForeignKey(x => x.SensorId);
            e.HasIndex(x => new { x.SensorId, x.Identifier }).IsUnique();
        });
        modelBuilder.Entity<SensorDataEntity>(e =>
        {
            e.ToTable("SensorData");
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Sensor)
                .WithMany()
                .HasPrincipalKey(x => x.Id)
                .HasForeignKey(x => x.SensorId)
                .OnDelete(DeleteBehavior.NoAction);
            e.Navigation(x => x.Sensor).AutoInclude();
            e.HasIndex(x => new { x.SensorId, x.AggregationType, x.CreatedAt }).IsUnique();
        });
    }
}