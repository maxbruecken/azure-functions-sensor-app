namespace AzureFunction.Core.Entities;

public class SensorInformationEntity
{
    public long Id { get; init; }
    
    public string Manufacturer { get; init; } = string.Empty;

    public string ModelNumber { get; init; } = string.Empty;

    public string SerialNumber { get; init; } = string.Empty;
}