#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class PublicationGeneratorExtensions
{
    public static Generator<Publication> DefaultPublication(this DataFixture fixture)
        => fixture.Generator<Publication>().WithDefaults();

    public static Generator<Publication> WithDefaults(this Generator<Publication> generator)
        => generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<Publication> SetDefaults(this InstanceSetters<Publication> setters)
        => setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Slug)
            .SetDefault(p => p.Summary)
            .SetDefault(p => p.Title);
    
    public static Generator<Publication> WithReleases(
        this Generator<Publication> generator,
        IEnumerable<Release> releases)
        => generator.ForInstance(s => s.SetReleases(releases));
    
    public static InstanceSetters<Publication> SetReleases(
        this InstanceSetters<Publication> setters,
        IEnumerable<Release> releases)
        => setters
            .Set(d => d.Releases, releases.ToList());

}
