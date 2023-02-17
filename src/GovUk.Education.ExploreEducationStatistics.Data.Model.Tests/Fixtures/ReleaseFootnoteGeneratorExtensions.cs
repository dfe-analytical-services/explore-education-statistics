#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class ReleaseFootnoteGeneratorExtensions
{
    public static Generator<ReleaseFootnote> DefaultReleaseFootnote(this DataFixture fixture)
        => fixture.Generator<ReleaseFootnote>().WithDefaults();

    public static Generator<ReleaseFootnote> WithDefaults(this Generator<ReleaseFootnote> generator)
        => generator.ForInstance(s => s.SetDefaults());
    
    public static InstanceSetters<ReleaseFootnote> SetDefaults(this InstanceSetters<ReleaseFootnote> setters)
        => setters
            .SetDefault(rf => rf.ReleaseId)
            .SetDefault(rf => rf.FootnoteId);
}
