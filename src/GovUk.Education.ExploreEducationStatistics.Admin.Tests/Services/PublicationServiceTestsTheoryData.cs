#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

internal class PublicationServiceTestsTheoryData
{
    private static readonly DataFixture DataFixture = new();

    private static Publication NotLivePublication() =>
        DataFixture.DefaultPublication().WithTheme(DataFixture.DefaultTheme().Generate());

    private static Publication LivePublication() =>
        DataFixture
            .DefaultPublication()
            .WithReleases([DataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)])
            .WithTheme(DataFixture.DefaultTheme());

    /// <summary>
    /// Test data to verify if expected events are raised during a publication update based on the publication's live
    /// state and the states of the initial and updated `SupersededBy` publications.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader>
    /// <term>parameter</term>
    /// <description>description</description>
    /// </listheader>
    /// <item>
    /// <term>publication</term>
    /// <description>The initial publication (live or not live).</description>
    /// </item>
    /// <item>
    /// <term>initialPublicationSupersededBy</term>
    /// <description>The initial `SupersededBy` publication (null, live, or not live).</description>
    /// </item>
    /// <item>
    /// <term>updatedPublicationSupersededBy</term>
    /// <description>The updated `SupersededBy` publication (null, live, or not live).</description>
    /// </item>
    /// <item>
    /// <term>expectPublicationArchivedEventRaised</term>
    /// <description>A boolean indicating whether a publication archived event is expected to be raised.</description>
    /// </item>
    /// <item>
    /// <term>expectedPublicationRestoredEventRaised</term>
    /// <description>A boolean indicating whether a publication restored event is expected to be raised.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static readonly TheoryData<
        Publication,
        Publication?,
        Publication?,
        bool,
        bool
    > PublicationArchivedEventTestData = new()
    {
        // csharpier-ignore-start
        // When the publication is live expect events to be raised dependent on states
        // of the initial and updated `SupersededBy` publications
        { LivePublication(), null, null, false, false },
        { LivePublication(), null, NotLivePublication(), false, false },
        // Transition to archived
        { LivePublication(), null, LivePublication(), true, false },
        { LivePublication(), NotLivePublication(), null, false, false },
        { LivePublication(), NotLivePublication(), NotLivePublication(), false, false },
        // Transition to archived
        { LivePublication(), NotLivePublication(), LivePublication(), true, false },
        // Transition to not archived
        { LivePublication(), LivePublication(), null, false, true },
        // Transition to not archived
        { LivePublication(), LivePublication(), NotLivePublication(), false, true },
        { LivePublication(), LivePublication(), LivePublication(), false, false },
        // When the publication is not live expect no events to be raised
        { NotLivePublication(), null, null, false, false },
        { NotLivePublication(), null, NotLivePublication(), false, false },
        { NotLivePublication(), null, LivePublication(), false, false },
        { NotLivePublication(), NotLivePublication(), null, false, false },
        { NotLivePublication(), NotLivePublication(), NotLivePublication(), false, false },
        { NotLivePublication(), NotLivePublication(), LivePublication(), false, false },
        { NotLivePublication(), LivePublication(), null, false, false },
        { NotLivePublication(), LivePublication(), NotLivePublication(), false, false },
        { NotLivePublication(), LivePublication(), LivePublication(), false, false }
        // csharpier-ignore-end
    };
}
