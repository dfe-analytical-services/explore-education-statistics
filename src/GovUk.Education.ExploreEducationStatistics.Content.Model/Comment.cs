using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Comment
    {
        public Guid Id { get; set; }
        public IContentBlock ContentBlock { get; set; }
        public Guid ContentBlockId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public User CreatedBy { get; set; }
        public Guid? CreatedById { get; set; }
        [Obsolete] public string LegacyCreatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}