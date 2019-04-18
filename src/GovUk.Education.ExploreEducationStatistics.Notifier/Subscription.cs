using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class SubscriptionEntity : TableEntity
    {
        public Boolean Verified { get; set; }
        public string Slug  { get; set; }        
        public string Title { get; set; }
        
        public SubscriptionEntity(string id, string email, string title, string slug)
        {
            PartitionKey = id;
            RowKey = email;
            Verified = false;
            this.Title = title;
            this.Slug = slug;
        }

        public SubscriptionEntity()
        {
        }
    }
}