using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class SubscriptionEntity : TableEntity
    {
        public Boolean Verified { get; set; }

        public SubscriptionEntity(string id, string email)
        {
            this.PartitionKey = "publication-" + id;
            this.RowKey = email;
            this.Verified = false;
        }

        public SubscriptionEntity()
        {
        }
    }
}