using System;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Comment
    {
        public Guid Id { get; set; }
        [JsonIgnore] public IContentBlock ContentBlock { get; set; }
        public Guid ContentBlockId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public Guid CreatedById { get; set; }
        [Obsolete] public string LegacyCreatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}