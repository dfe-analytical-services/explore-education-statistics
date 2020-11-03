using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Link
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public Link Clone()
        {
            var clone = MemberwiseClone() as Link;
            clone.Id = Guid.NewGuid();

            return clone;
        }
    }
}