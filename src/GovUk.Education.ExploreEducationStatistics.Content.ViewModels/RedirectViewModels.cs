namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record RedirectsViewModel(
    List<RedirectViewModel> PublicationRedirects,
    List<RedirectViewModel> MethodologyRedirects,
    Dictionary<string, List<RedirectViewModel>> ReleaseRedirectsByPublicationSlug
);

public record RedirectViewModel(string FromSlug, string ToSlug);
