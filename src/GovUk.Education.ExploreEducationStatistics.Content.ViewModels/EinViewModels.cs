using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersContentViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public class EducationInNumbersViewModels
{
    public record EinNavItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Slug { get; set; }

        public static EinNavItemViewModel FromModel(EinPageVersion pageVersion)
        {
            return new EinNavItemViewModel
            {
                Title = pageVersion.EinPage.Title,
                Slug = pageVersion.EinPage.Slug,
                Order = pageVersion.EinPage.Order,
            };
        }
    }

    public record EinPageVersionViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset Published { get; set; }
        public List<EinContentSectionViewModel> Content { get; set; } = null!;

        public static EinPageVersionViewModel FromModel(EinPageVersion pageVersion)
        {
            return new EinPageVersionViewModel
            {
                Id = pageVersion.Id,
                Title = pageVersion.EinPage.Title,
                Slug = pageVersion.EinPage.Slug,
                Description = pageVersion.EinPage.Description,
                Published = pageVersion.Published!.Value, // we only display published Ein pages publicly
                Content = pageVersion
                    .Content.Select(EinContentSectionViewModel.FromModel)
                    .OrderBy(section => section.Order)
                    .ToList(),
            };
        }
    }

    public record EinPageSitemapItemViewModel
    {
        public required string Slug { get; set; }
        public required DateTimeOffset LastModified { get; set; }

        public static EinPageSitemapItemViewModel FromModel(EinPageVersion pageVersion)
        {
            return new EinPageSitemapItemViewModel
            {
                Slug = pageVersion.EinPage.Slug ?? string.Empty, // ein root page has a null slug - we want an empty string in that case
                LastModified = pageVersion.Updated ?? pageVersion.Created,
            };
        }
    }
}
