using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ThemeMockBuilder
{
    public Theme Build() =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Theme Title",
            Summary = "Theme Summary",
            Slug = "theme-slug",
            Publications = []
        };
}
