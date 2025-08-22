using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public class SubscriptionEntity : ITableEntity
{
    public string? PartitionKey { get; set; }

    [IgnoreDataMember]
    public string? PublicationId => PartitionKey;

    public string? RowKey { get; set; }
    public string? Email => RowKey;

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? DateTimeCreated { get; set; } // For pending subs, this is used to store the time it expires, not the created date!

}
