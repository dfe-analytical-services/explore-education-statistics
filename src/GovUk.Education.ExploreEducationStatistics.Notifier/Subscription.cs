#nullable enable
using System;
using Azure;
using Azure.Data.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class SubscriptionEntity : ITableEntity
    {
        public string Slug { get; set; } = default!;
        public string Title { get; set; } = default!;
        public DateTime? DateTimeCreated { get; set; }

        public string PartitionKey { get; set; } = default!;
        public string RowKey { get; set; } = default!;
        public ETag ETag { get; set; } = default!;
        public DateTimeOffset? Timestamp { get; set; } = default!;

        public SubscriptionEntity(string id, string email, string title, string slug, DateTime? dateTimeCreated)
        {
            PartitionKey = id;
            RowKey = email;
            Title = title;
            Slug = slug;
            DateTimeCreated = dateTimeCreated;
        }

        public SubscriptionEntity(string id, string email)
        {
            PartitionKey = id;
            RowKey = email;
        }

        public SubscriptionEntity()
        {
        }
    }
}
