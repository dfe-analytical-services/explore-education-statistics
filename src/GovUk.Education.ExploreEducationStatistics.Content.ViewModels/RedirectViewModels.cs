namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record RedirectsViewModel(
    List<RedirectViewModel> Publications,
    List<RedirectViewModel> Methodologies);

public record RedirectViewModel(
    string FromSlug,
    string ToSlug);
