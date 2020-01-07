using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Core.Models
{
    public class SensorAlarm : TableEntity
    {
        public SensorAlarm()
        {
            RowKey = Guid.NewGuid().ToString();
        }

        [IgnoreProperty]
        public string SensorId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public DateTimeOffset FiredAt { get; set; } = DateTimeOffset.UtcNow;

        [IgnoreProperty]
        public AlarmStatus Status { get; set; }

        public string StatusString
        {
            get { return Status.ToString(); }
            set { Status = (AlarmStatus)Enum.Parse(typeof(AlarmStatus), value); }
        }
    }

    public enum AlarmStatus
    {
        InvalidData, Dead, Closed
    }
}
