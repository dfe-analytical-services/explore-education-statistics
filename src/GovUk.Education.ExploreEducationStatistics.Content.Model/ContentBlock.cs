using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public abstract class ContentBlock
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public DateTime? Created { get; set; }

        public List<Comment> Comments { get; set; }

        public ContentBlock Clone(Release.CloneContext context, ContentSection? newContentSection)
        {
            var copy = MemberwiseClone() as ContentBlock;
            // copy.Id = Guid.NewGuid();
            //
            // if (newContentSection != null)
            // {
            //     copy.ContentSection = newContentSection;
            //     copy.ContentSectionId = newContentSection.Id;
            // }
            //
            // // start a new amendment with no comments
            // copy.Comments = new List<Comment>();
            //
            // context.ContentBlocks.Add(this, copy);

            return copy;
        }
        
        public ContentBlock Clone(DateTime createdDate)
        {
            var copy = MemberwiseClone() as ContentBlock;
            // copy.Id = Guid.NewGuid();
            // copy.Created = createdDate;
            // copy.ContentSection = null;
            // copy.ContentSectionId = null;
            //
            // // start a new amendment with no comments
            // copy.Comments = new List<Comment>();
            return copy;
        }
    }
}
