using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class PublishMethodologyMessage
    {
        public Guid MethodologyId { get; set; }

        public PublishMethodologyMessage(Guid methodologyId)
        {
            MethodologyId = methodologyId;
        }

        public override string ToString()
        {
            return $"{nameof(MethodologyId)}: {MethodologyId}";
        }
    }
}