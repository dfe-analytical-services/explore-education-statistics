#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record RedirectsViewModel(
    List<RedirectViewModel> Publications,
    List<RedirectViewModel> Methodologies);

public record RedirectViewModel(
    string FromSlug,
    string ToSlug);
