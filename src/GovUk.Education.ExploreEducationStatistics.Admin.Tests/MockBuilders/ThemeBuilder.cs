using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ThemeBuilder(Guid? themeId = null)
{
    public Theme Build() =>
        new()
        {
            Id = themeId ?? Guid.NewGuid(),
            Title = "Theme Title",
            Summary = "Theme Summary",
            Slug = "theme-slug",
            Publications = []
        };
}
