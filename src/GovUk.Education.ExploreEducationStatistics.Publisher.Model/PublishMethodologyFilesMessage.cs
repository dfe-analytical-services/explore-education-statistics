using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishMethodologyFilesMessage
    {
        public Guid MethodologyId { get; set; }

        public PublishMethodologyFilesMessage(Guid methodologyId)
        {
            MethodologyId = methodologyId;
        }

        public override string ToString()
        {
            return $"{nameof(MethodologyId)}: {MethodologyId}";
        }
    }
}
