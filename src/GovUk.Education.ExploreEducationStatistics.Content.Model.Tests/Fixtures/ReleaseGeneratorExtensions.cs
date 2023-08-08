#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ReleaseGeneratorExtensions
{
    public static Generator<Release> DefaultRelease(this DataFixture fixture)
        => fixture.Generator<Release>().WithDefaults();

    public static Generator<Release> WithDefaults(this Generator<Release> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Title)
            .SetDefault(p => p.DataGuidance)
            .SetDefault(p => p.ReleaseName)
            .SetDefault(p => p.Year);
}
