#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static System.String;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class PublicationCreateViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = Empty;

        public string Summary { get; set; } = Empty;

        public string Slug { get; set; } = Empty;

        public Contact Contact { get; set; } = null!;

        public IdTitleViewModel Topic { get; set; } = null!;

        public IdTitleViewModel Theme { get; set; } = null!;

        public Guid? SupersededById { get; set; }

        public bool IsSuperseded { get; set; }
    }
}
