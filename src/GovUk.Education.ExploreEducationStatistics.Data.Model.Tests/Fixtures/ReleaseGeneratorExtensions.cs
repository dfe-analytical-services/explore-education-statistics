#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class ReleaseGeneratorExtensions
{
    public static Generator<Release> DefaultStatsRelease(this DataFixture fixture)
        => fixture.Generator<Release>().WithDefaults();

    public static Generator<Release> WithDefaults(this Generator<Release> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<Release> WithId(this Generator<Release> generator, Guid id)
        => generator.ForInstance(s => s.SetId(id));

    public static InstanceSetters<Release> SetDefaults(this InstanceSetters<Release> setters)
        => setters
            .SetDefault(r => r.Id)
            .SetDefault(r => r.PublicationId);

    public static InstanceSetters<Release> SetId(this InstanceSetters<Release> setters, Guid id)
        => setters.Set(r => r.Id, id);
}
