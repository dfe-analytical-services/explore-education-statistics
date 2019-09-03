using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class SubscriptionEntity : TableEntity
    {
        public string Slug { get; set; }
        public string Title { get; set; }
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