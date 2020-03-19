using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class BasicLink
    {
        public Guid Id { get; set; }
        
        public string Description { get; set; }

        public string Url { get; set; }

        public BasicLink CreateCopy()
        {
            return MemberwiseClone() as BasicLink;
        }
    }
}