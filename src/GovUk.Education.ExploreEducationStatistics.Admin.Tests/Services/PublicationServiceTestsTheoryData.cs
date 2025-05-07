#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

internal class PublicationServiceTestsTheoryData
{
    private static readonly DataFixture DataFixture = new();

    private static Publication NotLivePublication() => DataFixture
        .DefaultPublication()
        .WithTheme(DataFixture.DefaultTheme().Generate());

    private static Publication LivePublication() => DataFixture
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
    /// </list>
    /// </remarks>
    public static readonly TheoryData<Publication, Publication?, Publication?, bool>
        PublicationArchivedEventTestData =
            new()
            {
                // When the publication is live expect events to be raised dependent on states
                // of the initial and updated `SupersededBy` publications
                { LivePublication(), null, null, false },
                { LivePublication(), null, NotLivePublication(), false },
                { LivePublication(), null, LivePublication(), true }, // Transition to archived
                { LivePublication(), NotLivePublication(), null, false },
                { LivePublication(), NotLivePublication(), NotLivePublication(), false },
                { LivePublication(), NotLivePublication(), LivePublication(), true }, // Transition to archived
                { LivePublication(), LivePublication(), null, false },
                { LivePublication(), LivePublication(), NotLivePublication(), false },
                { LivePublication(), LivePublication(), LivePublication(), false },
                // When the publication is not live expect no events to be raised
                { NotLivePublication(), null, null, false },
                { NotLivePublication(), null, NotLivePublication(), false },
                { NotLivePublication(), null, LivePublication(), false },
                { NotLivePublication(), NotLivePublication(), null, false },
                { NotLivePublication(), NotLivePublication(), NotLivePublication(), false },
                { NotLivePublication(), NotLivePublication(), LivePublication(), false },
                { NotLivePublication(), LivePublication(), null, false },
                { NotLivePublication(), LivePublication(), NotLivePublication(), false },
                { NotLivePublication(), LivePublication(), LivePublication(), false }
            };
}
