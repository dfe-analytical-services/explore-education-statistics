#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Utils.PublicationArchiveStatusTransitionResolver.PublicationArchiveStatusTransition;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;

public abstract class PublicationArchiveStatusTransitionResolverTests
{
    public class GetTransitionTests : PublicationArchiveStatusTransitionResolverTests
    {
        private static readonly Publication LivePublication = CreatePublication(live: true);
        private static readonly Publication NotLivePublication = CreatePublication(live: false);

        private static Publication CreatePublication(bool live)
        {
            return new Publication
            {
                LatestPublishedReleaseVersionId = live ? Guid.NewGuid() : null,
            };
        }

        public static readonly TheoryData<
            Publication?,
            Publication?,
            PublicationArchiveStatusTransitionResolver.PublicationArchiveStatusTransition
        > GetTransitionTestCases = new()
        {
            { null, null, NotArchivedToNotArchived },
            { null, NotLivePublication, NotArchivedToNotArchived },
            { null, LivePublication, NotArchivedToArchived },
            { NotLivePublication, null, NotArchivedToNotArchived },
            { NotLivePublication, NotLivePublication, NotArchivedToNotArchived },
            { NotLivePublication, LivePublication, NotArchivedToArchived },
            { LivePublication, null, ArchivedToNotArchived },
            { LivePublication, NotLivePublication, ArchivedToNotArchived },
            { LivePublication, LivePublication, ArchivedToArchived },
        };

        [Theory]
        [MemberData(nameof(GetTransitionTestCases))]
        public void GetTransition_ReturnsExpectedTransition(
            Publication? beforeSupersededBy,
            Publication? afterSupersededBy,
            PublicationArchiveStatusTransitionResolver.PublicationArchiveStatusTransition expected
        )
        {
            var result = PublicationArchiveStatusTransitionResolver.GetTransition(
                beforeSupersededBy,
                afterSupersededBy
            );

            Assert.Equal(expected, result);
        }
    }
}
