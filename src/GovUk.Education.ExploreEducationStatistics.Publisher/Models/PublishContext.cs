using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Models
{
    public class PublishContext
    {
        public DateTime Published { get; }
        public bool Staging { get; }

        public PublishContext(DateTime published, bool staging)
        {
            Published = published;
            Staging = staging;
        }
    }
}