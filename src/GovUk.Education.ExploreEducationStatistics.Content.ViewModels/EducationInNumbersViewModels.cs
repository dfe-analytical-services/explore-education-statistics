using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersContentViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public class EducationInNumbersViewModels
{
    public record EinNavItemViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Slug { get; set; }
        public DateTimeOffset Published { get; set; }

        public static EinNavItemViewModel FromModel(EducationInNumbersPage page)
        {
            return new EinNavItemViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Published = page.Published!.Value, // we only display published Ein pages publicly
                Order = page.Order,
            };
        }
    }

    public record EinPageViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset Published { get; set; }
        public List<EinContentSectionViewModel> Content { get; set; } = null!;

        public static EinPageViewModel FromModel(EducationInNumbersPage page)
        {
            return new EinPageViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Description = page.Description,
                Published = page.Published!.Value, // we only display published Ein pages publicly
                Content = page
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

        public static EinPageSitemapItemViewModel FromModel(EducationInNumbersPage page)
        {
            return new EinPageSitemapItemViewModel
            {
                Slug = page.Slug ?? string.Empty, // ein root page has a null slug - we want an empty string in that case
                LastModified = page.Updated ?? page.Created,
            };
        }
    }
}
