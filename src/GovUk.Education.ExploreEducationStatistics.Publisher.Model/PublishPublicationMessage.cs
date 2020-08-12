using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishPublicationMessage
    {
        public Guid PublicationId { get; set; }
        public string OldSlug { get; set; }

        public PublishPublicationMessage(Guid publicationId, string oldSlug)
        {
            PublicationId = publicationId;
            OldSlug = oldSlug;
        }

        public override string ToString()
        {
            return $"{nameof(PublicationId)}: {PublicationId}, {nameof(OldSlug)}: {OldSlug}";
        }
    }
}