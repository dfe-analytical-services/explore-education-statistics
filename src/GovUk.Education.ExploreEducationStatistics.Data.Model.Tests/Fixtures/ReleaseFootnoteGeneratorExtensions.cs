#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class ReleaseFootnoteGeneratorExtensions
{
    public static Generator<ReleaseFootnote> DefaultReleaseFootnote(this DataFixture fixture) =>
        fixture.Generator<ReleaseFootnote>().WithDefaults();

    public static Generator<ReleaseFootnote> WithDefaults(
        this Generator<ReleaseFootnote> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static InstanceSetters<ReleaseFootnote> SetDefaults(
        this InstanceSetters<ReleaseFootnote> setters
    ) => setters.SetDefault(rf => rf.ReleaseVersionId).SetDefault(rf => rf.FootnoteId);

    public static Generator<ReleaseFootnote> WithReleaseVersion(
        this Generator<ReleaseFootnote> generator,
        ReleaseVersion releaseVersion
    ) => generator.ForInstance(s => s.SetReleaseVersion(releaseVersion));

    public static Generator<ReleaseFootnote> WithFootnote(
        this Generator<ReleaseFootnote> generator,
        Footnote footnote
    ) => generator.ForInstance(s => s.SetFootnote(footnote));

    public static Generator<ReleaseFootnote> WithFootnotes(
        this Generator<ReleaseFootnote> generator,
        IEnumerable<Footnote> footnotes
    )
    {
        footnotes.ForEach(
            (footnote, index) => generator.ForIndex(index, s => s.SetFootnote(footnote))
        );

        return generator;
    }

    public static InstanceSetters<ReleaseFootnote> SetReleaseVersion(
        this InstanceSetters<ReleaseFootnote> setters,
        ReleaseVersion releaseVersion
    ) => setters.Set(rf => rf.ReleaseVersion, releaseVersion);

    public static InstanceSetters<ReleaseFootnote> SetFootnote(
        this InstanceSetters<ReleaseFootnote> setters,
        Footnote footnote
    ) => setters.Set(rf => rf.Footnote, footnote);
}
