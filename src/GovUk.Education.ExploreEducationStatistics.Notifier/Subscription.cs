using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class SubscriptionEntity : TableEntity
    {
        public Boolean Verified { get; set; }
        public string Slug  { get; set; }        
        public string title { get; set; }
        
        public SubscriptionEntity(string id, string email, string title, string slug)
        {
            PartitionKey = id;
            RowKey = email;
            Verified = false;
            this.title = title;
            this.Slug = slug;
        }

        public SubscriptionEntity()
        {
        }
    }
}