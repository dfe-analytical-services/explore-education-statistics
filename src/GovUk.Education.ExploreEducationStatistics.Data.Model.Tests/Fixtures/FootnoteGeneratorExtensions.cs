#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class FootnoteGeneratorExtensions
{
    public static Generator<Footnote> DefaultFootnote(this DataFixture fixture)
        => fixture.Generator<Footnote>().WithDefaults();

    public static Generator<Footnote> WithDefaults(this Generator<Footnote> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<Footnote> SetDefaults(this InstanceSetters<Footnote> setters)
        => setters
            .SetDefault(f => f.Id)
            .SetDefault(f => f.Content)
            .Set(f => f.Order, f => f.IndexFaker)
            .Set(f => f.Created, f => f.Date.Past())
            .Set(
                f => f.Updated,
                (f, footnote) => f.Date.Soon(14, footnote.Created)
            );
}
