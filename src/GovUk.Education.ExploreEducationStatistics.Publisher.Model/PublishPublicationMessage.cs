using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishPublicationMessage
    {
        public Guid PublicationId { get; set; }

        public PublishPublicationMessage(Guid publicationId)
        {
            PublicationId = publicationId;
        }

        public override string ToString()
        {
            return $"{nameof(PublicationId)}: {PublicationId}";
        }
    }
}