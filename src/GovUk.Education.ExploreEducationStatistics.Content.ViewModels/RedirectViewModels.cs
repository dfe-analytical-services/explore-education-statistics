namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record RedirectsViewModel(
    List<RedirectViewModel> Publications,
    List<RedirectViewModel> Methodologies,
    List<RedirectViewModel> Releases);

public record RedirectViewModel(
    string FromSlug,
    string ToSlug);
