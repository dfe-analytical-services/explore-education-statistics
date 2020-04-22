using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Link
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public Link CreateCopy()
        {
            return MemberwiseClone() as Link;
        }
    }
}