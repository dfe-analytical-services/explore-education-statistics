using System;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Comment
    {
        public Guid Id { get; set; }
        // TODO EES-18 This JsonIgnore can be removed when the view model for ContentBlocks is added
        [JsonIgnore] public IContentBlock ContentBlock { get; set; }
        public Guid ContentBlockId { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public User CreatedBy { get; set; }
        public Guid? CreatedById { get; set; }
        [Obsolete] public string LegacyCreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        
        // TODO EES-18 Remove when introducing CommentViewModel
        public string CreatedByName => CreatedById.HasValue ? $"{CreatedBy.FirstName} {CreatedBy.LastName}" : LegacyCreatedBy;
    }
}