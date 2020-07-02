using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ThemeViewModel
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
    }
}