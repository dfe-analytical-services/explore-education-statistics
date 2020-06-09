using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class MarkDownBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public List<CommentViewModel> Comments { get; set; }

        public int Order { get; set; }

        public string Body { get; set; }

        public ContentBlockType Type => ContentBlockType.MarkDownBlock;
    }
}