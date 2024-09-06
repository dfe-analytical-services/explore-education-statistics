using System;
using Azure;
using Azure.Data.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class SubscriptionEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? DateTimeCreated { get; set; }
    }

    public class SubscriptionEntityOld : Microsoft.Azure.Cosmos.Table.TableEntity // @MarkFix remove
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? DateTimeCreated { get; set; }

        public SubscriptionEntityOld(string id, string email, string title, string slug, DateTime? dateTimeCreated)
        {
            PartitionKey = id;
            RowKey = email;
            Title = title;
            Slug = slug;
            DateTimeCreated = dateTimeCreated;
        }

        public SubscriptionEntityOld(string id, string email)
        {
            PartitionKey = id;
            RowKey = email;
        }

        public SubscriptionEntityOld()
        {
        }
    }
}
