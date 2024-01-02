#nullable enable
using System;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class SubscriptionEntity : TableEntity
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? DateTimeCreated { get; set; }

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
