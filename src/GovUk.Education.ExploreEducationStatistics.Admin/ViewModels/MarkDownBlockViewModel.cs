using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    [JsonKnownThisType(nameof(MarkDownBlock))]
    public class MarkDownBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public List<CommentViewModel> Comments { get; set; }

        public int Order { get; set; }

        public string Body { get; set; }
    }
}