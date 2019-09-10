using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model
{
    public class PublicationNotificationMessage
    {
        public Guid PublicationId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
    }
}