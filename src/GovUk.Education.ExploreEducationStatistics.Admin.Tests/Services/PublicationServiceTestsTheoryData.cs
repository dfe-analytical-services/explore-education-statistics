#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

internal class PublicationServiceTestsTheoryData
{
    private static readonly DataFixture DataFixture = new();

    private static readonly Func<Publication?> NullPublication = () => null;

    private static readonly Func<Publication> NotLivePublication = () => DataFixture
        .DefaultPublication()
        .WithTheme(DataFixture.DefaultTheme());

    private static readonly Func<Publication> LivePublication = () => DataFixture
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
    /// <term>initialPublicationGenerator</term>
    /// <description>A generator for the initial publication (live or not live).</description>
    /// </item>
    /// <item>
    /// <term>initialPublicationSupersededByGenerator</term>
    /// <description>A generator for the initial `SupersededBy` publication (null, live, or not live).</description>
    /// </item>
    /// <item>
    /// <term>updatedPublicationSupersededByGenerator</term>
    /// <description>A generator for the new `SupersededBy` publication (null, live, or not live).</description>
    /// </item>
    /// <item>
    /// <term>expectPublicationArchivedEventRaised</term>
    /// <description>A boolean indicating whether a publication archived event is expected to be raised.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static readonly TheoryData<Func<Publication>, Func<Publication?>, Func<Publication?>, bool>
        PublicationArchivedEventTestData =
            new()
            {
                // When the publication is live expect events to be raised dependent on states
                // of the initial and updated `SupersededBy` publications
                { LivePublication, NullPublication, NullPublication, false },
                { LivePublication, NullPublication, NotLivePublication, false },
                { LivePublication, NullPublication, LivePublication, true }, // Transition to archived
                { LivePublication, NotLivePublication, NullPublication, false },
                { LivePublication, NotLivePublication, NotLivePublication, false },
                { LivePublication, NotLivePublication, LivePublication, true }, // Transition to archived
                { LivePublication, LivePublication, NullPublication, false },
                { LivePublication, LivePublication, NotLivePublication, false },
                { LivePublication, LivePublication, LivePublication, false },
                // When the publication is not live expect no events to be raised
                { NotLivePublication, NullPublication, NullPublication, false },
                { NotLivePublication, NullPublication, NotLivePublication, false },
                { NotLivePublication, NullPublication, LivePublication, false },
                { NotLivePublication, NotLivePublication, NullPublication, false },
                { NotLivePublication, NotLivePublication, NotLivePublication, false },
                { NotLivePublication, NotLivePublication, LivePublication, false },
                { NotLivePublication, LivePublication, NullPublication, false },
                { NotLivePublication, LivePublication, NotLivePublication, false },
                { NotLivePublication, LivePublication, LivePublication, false }
            };
}
