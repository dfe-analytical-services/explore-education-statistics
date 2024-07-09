using Azure;
using Azure.Data.Tables;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public class ApiSubscription : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string DataSetTitle { get; set; }
    public ApiSubscriptionStatus Status { get; set; }
    public DateTimeOffset? ExpiryDateTime { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
