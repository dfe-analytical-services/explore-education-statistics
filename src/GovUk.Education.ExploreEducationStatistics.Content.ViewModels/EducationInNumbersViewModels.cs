using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersContentViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public class EducationInNumbersViewModels
{
    public class EinNavItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Slug { get; set; }
        public DateTimeOffset Published { get; set; }
    }

    public class EinPageViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset Published { get; set; }
        public List<EinContentSectionViewModel> Content { get; set; } = null!;
    }
}
