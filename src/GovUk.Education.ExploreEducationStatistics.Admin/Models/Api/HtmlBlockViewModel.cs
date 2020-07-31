using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    [JsonKnownThisType(nameof(HtmlBlock))]
    public class HtmlBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public List<CommentViewModel> Comments { get; set; }

        public int Order { get; set; }

        public string Body { get; set; }
    }
}