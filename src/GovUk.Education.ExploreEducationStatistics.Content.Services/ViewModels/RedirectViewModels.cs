#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record RedirectsViewModel(
    List<MethodologyRedirectViewModel> MethodologyRedirects);

public record MethodologyRedirectViewModel(
    string FromSlug,
    string ToSlug);

