using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public abstract class ContentSection
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; }

        public string Caption { get; set; }

        public ContentSection Clone(ReleaseContentSection newReleaseContentSection, Release.CloneContext context)
        {
            var copy = MemberwiseClone() as ContentSection;
            // copy.Id = Guid.NewGuid();
            //
            // copy.Release = newReleaseContentSection;
            //
            // copy.Content = copy
            //     .Content?
            //     .Select(content => content.Clone(context, copy))
            //     .ToList();
            //
            return copy;
        }
        
        public ContentSection Clone(DateTime createdDate)
        {
            var copy = MemberwiseClone() as ContentSection;
            // copy.Id = Guid.NewGuid();
            //
            // copy.Content = copy
            //     .Content?
            //     .Select(content => content.Clone(createdDate))
            //     .ToList();
            //
            // copy.Content?.ForEach(c => c.ContentSectionId = copy.Id);
            return copy;
        }
    }
}
