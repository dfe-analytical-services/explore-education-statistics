#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class NewPermissionsSystemHelperTests
{
    // (publicationRoleToCreate, existingPublicationRoles, existingReleaseRoles, expectedNewSystemPublicationRoleToRemove, expectedNewSystemPublicationRoleToCreate)
    public static TheoryData<
        PublicationRole,
        HashSet<PublicationRole>,
        HashSet<ReleaseRole>,
        PublicationRole?,
        PublicationRole?
    > PublicationRoleCreationData =>
        new()
        {
            // csharpier-ignore-start
            { PublicationRole.Owner, [], [], null, PublicationRole.Drafter },
            { PublicationRole.Owner, [], [ReleaseRole.PrereleaseViewer], null, PublicationRole.Drafter },
            { PublicationRole.Owner, [], [ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { PublicationRole.Owner, [], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Owner, [], [ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower], [], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Approver], [], PublicationRole.Approver, PublicationRole.Drafter },
            { PublicationRole.Owner, [PublicationRole.Drafter], [], null, null },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Approver], [], null, null },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower], [ReleaseRole.Approver, ReleaseRole.Contributor], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Approver], [ReleaseRole.Contributor], PublicationRole.Approver, PublicationRole.Drafter },
            { PublicationRole.Owner, [PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null},
            { PublicationRole.Owner, [PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { PublicationRole.Owner, [PublicationRole.Approver], [ReleaseRole.Approver, ReleaseRole.Contributor], null, null },
            { PublicationRole.Owner, [PublicationRole.Drafter], [ReleaseRole.Contributor], null, null },
            { PublicationRole.Owner, [PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Drafter], [ReleaseRole.Approver, ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver, ReleaseRole.Contributor], null, null },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver, ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [], [], null, PublicationRole.Approver },
            { PublicationRole.Allower, [], [ReleaseRole.PrereleaseViewer], null, PublicationRole.Approver },
            { PublicationRole.Allower, [], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { PublicationRole.Allower, [], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Allower, [], [ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner], [], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Approver], [], null, null },
            { PublicationRole.Allower, [PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Approver], [], null, null },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner], [ReleaseRole.Approver, ReleaseRole.Contributor], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { PublicationRole.Allower, [PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null},
            { PublicationRole.Allower, [PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { PublicationRole.Allower, [PublicationRole.Approver], [ReleaseRole.Approver, ReleaseRole.Contributor], null, null },
            { PublicationRole.Allower, [PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Drafter], [ReleaseRole.Approver, ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Approver, ReleaseRole.Contributor], null, null },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Approver, ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            // csharpier-ignore-end
        };

    // (releaseRoleToCreate, existingPublicationRoles, existingReleaseRoles, expectedNewSystemPublicationRoleToRemove, expectedNewSystemPublicationRoleToCreate)
    public static TheoryData<
        ReleaseRole,
        HashSet<PublicationRole>,
        HashSet<ReleaseRole>,
        PublicationRole?,
        PublicationRole?
    > ReleaseRoleCreationData =>
        new()
        {
            // csharpier-ignore-start
            { ReleaseRole.PrereleaseViewer, [], [], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner], [], null, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower], [], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Approver], [], PublicationRole.Approver, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Drafter], [], PublicationRole.Drafter, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Approver], [], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Drafter], [], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Approver], [], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [], [ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner], [ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Approver], [ReleaseRole.Contributor], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Drafter], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [], [], null, PublicationRole.Drafter },
            { ReleaseRole.Contributor, [PublicationRole.Owner], [], null, PublicationRole.Drafter },
            { ReleaseRole.Contributor, [PublicationRole.Allower], [], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Approver], [], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.Contributor, [PublicationRole.Drafter], [], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Approver], [], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Drafter], [], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Approver], [], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Approver], [], null, null },
            { ReleaseRole.Contributor, [], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Owner], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Allower], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [], [], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner], [], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower], [], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Approver], [], null, null },
            { ReleaseRole.Approver, [PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Approver], [], null, null },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Approver], [], null, null },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Approver], [], null, null },
            { ReleaseRole.Approver, [], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.Approver, [PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            // csharpier-ignore-end
        };

    // (oldPublicationRoleToRemove, existingPublicationRoles, existingReleaseRoles, expectedNewSystemPublicationRoleToRemove, expectedNewSystemPublicationRoleToCreate)
    public static TheoryData<
        PublicationRole,
        HashSet<PublicationRole>,
        HashSet<ReleaseRole>,
        PublicationRole?,
        PublicationRole?
    > PublicationRoleRemovalData =>
        new()
        {
            // csharpier-ignore-start
            { PublicationRole.Owner, [PublicationRole.Owner], [], null, null },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower], [], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Drafter], [], PublicationRole.Drafter, null },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Approver], [], PublicationRole.Approver, null },
            { PublicationRole.Owner, [PublicationRole.Owner], [ReleaseRole.PrereleaseViewer], null, null },
            { PublicationRole.Owner, [PublicationRole.Owner], [ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { PublicationRole.Owner, [PublicationRole.Owner], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor], null, null },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor], PublicationRole.Approver, PublicationRole.Drafter },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [], null, null },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { PublicationRole.Owner, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Allower], [], null, null },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner], [], null, PublicationRole.Drafter },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Drafter], [], PublicationRole.Drafter, null },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Approver], [], PublicationRole.Approver, null },
            { PublicationRole.Allower, [PublicationRole.Allower], [ReleaseRole.PrereleaseViewer], null, null },
            { PublicationRole.Allower, [PublicationRole.Allower], [ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { PublicationRole.Allower, [PublicationRole.Allower], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner], [ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], null, null },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], PublicationRole.Approver, PublicationRole.Drafter },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Approver], [], PublicationRole.Approver, PublicationRole.Drafter },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Drafter], [], null, null },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor], PublicationRole.Approver, PublicationRole.Drafter },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor], null, null },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { PublicationRole.Allower, [PublicationRole.Allower, PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            // csharpier-ignore-end
        };

    // (releaseRoleToRemove, existingPublicationRoles, existingReleaseRoles, expectedNewSystemPublicationRoleToRemove, expectedNewSystemPublicationRoleToCreate)
    public static TheoryData<
        ReleaseRole,
        HashSet<PublicationRole>,
        HashSet<ReleaseRole>,
        PublicationRole?,
        PublicationRole?
    > ReleaseRoleRemovalData =>
        new()
        {
            // csharpier-ignore-start
            { ReleaseRole.PrereleaseViewer, [], [ReleaseRole.PrereleaseViewer], null, null },
            { ReleaseRole.PrereleaseViewer, [], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [], [ReleaseRole.PrereleaseViewer, ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver, ReleaseRole.Contributor], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner], [ReleaseRole.PrereleaseViewer], null, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower], [ReleaseRole.PrereleaseViewer], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Drafter], [ReleaseRole.PrereleaseViewer], PublicationRole.Drafter, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Approver], [ReleaseRole.PrereleaseViewer], PublicationRole.Approver, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Drafter], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Approver], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower], [ReleaseRole.PrereleaseViewer], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.PrereleaseViewer], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.PrereleaseViewer], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.PrereleaseViewer], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.PrereleaseViewer], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.PrereleaseViewer], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.PrereleaseViewer], null, null},
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], null, null },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.PrereleaseViewer, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.PrereleaseViewer, ReleaseRole.Approver], null, null},
            { ReleaseRole.Contributor, [], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.Contributor, [], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [], [ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer], null, null },
            { ReleaseRole.Contributor, [], [ReleaseRole.Contributor, ReleaseRole.Approver, ReleaseRole.PrereleaseViewer], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Owner], [ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { ReleaseRole.Contributor, [PublicationRole.Allower], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, null },
            { ReleaseRole.Contributor, [PublicationRole.Approver], [ReleaseRole.Contributor], PublicationRole.Approver, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Allower], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Allower], [ReleaseRole.Contributor], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor], null, null},
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Allower], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Contributor, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null},
            { ReleaseRole.Approver, [], [ReleaseRole.Approver], null, null },
            { ReleaseRole.Approver, [], [ReleaseRole.Approver, ReleaseRole.Contributor], null, PublicationRole.Drafter },
            { ReleaseRole.Approver, [], [ReleaseRole.Approver, ReleaseRole.PrereleaseViewer], null, null },
            { ReleaseRole.Approver, [], [ReleaseRole.Approver, ReleaseRole.Contributor, ReleaseRole.PrereleaseViewer], null, PublicationRole.Drafter },
            { ReleaseRole.Approver, [PublicationRole.Owner], [ReleaseRole.Approver], null, PublicationRole.Drafter },
            { ReleaseRole.Approver, [PublicationRole.Allower], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, null },
            { ReleaseRole.Approver, [PublicationRole.Approver], [ReleaseRole.Approver], PublicationRole.Approver, null },
            { ReleaseRole.Approver, [PublicationRole.Owner], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Drafter },
            { ReleaseRole.Approver, [PublicationRole.Allower], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.Approver, [PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Allower], [ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Approver], null, null },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Approver], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Approver], null, null},
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Allower], [ReleaseRole.Contributor, ReleaseRole.Approver], null, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Approver, PublicationRole.Drafter },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Drafter], [ReleaseRole.Contributor, ReleaseRole.Approver], PublicationRole.Drafter, PublicationRole.Approver },
            { ReleaseRole.Approver, [PublicationRole.Owner, PublicationRole.Allower, PublicationRole.Approver], [ReleaseRole.Contributor, ReleaseRole.Approver], null, null},
            // csharpier-ignore-end
        };

    [Theory]
    [MemberData(nameof(PublicationRoleCreationData))]
    public void DetermineNewPermissionsSystemChangesForRoleCreation_ForPublicationRole(
        PublicationRole publicationRoleToCreate,
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove,
        PublicationRole? expectedNewSystemPublicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleCreation(
                oldPublicationRoleToCreate: publicationRoleToCreate,
                existingPublicationRoles: existingPublicationRoles,
                existingReleaseRoles: existingReleaseRoles
            );

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
        Assert.Equal(expectedNewSystemPublicationRoleToCreate, newSystemPublicationRoleToCreate);
    }

    [Theory]
    [InlineData(PublicationRole.Approver)]
    [InlineData(PublicationRole.Drafter)]
    public void DetermineNewPermissionsSystemChangesForRoleCreation_ForNewPublicationRole_Throws(
        PublicationRole publicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleCreation(
                oldPublicationRoleToCreate: publicationRoleToCreate,
                existingPublicationRoles: [],
                existingReleaseRoles: []
            )
        );
    }

    [Theory]
    [InlineData(PublicationRole.Allower)]
    [InlineData(PublicationRole.Owner)]
    public void DetermineNewPermissionsSystemChangesForRoleCreation_ForExistingPublicationRole_Throws(
        PublicationRole publicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleCreation(
                oldPublicationRoleToCreate: publicationRoleToCreate,
                existingPublicationRoles: [publicationRoleToCreate],
                existingReleaseRoles: []
            )
        );
    }

    [Theory]
    [MemberData(nameof(ReleaseRoleCreationData))]
    public void DetermineNewPermissionsSystemChangesForRoleCreation_ForReleaseRole(
        ReleaseRole releaseRoleToCreate,
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove,
        PublicationRole? expectedNewSystemPublicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleCreation(
                releaseRoleToCreate: releaseRoleToCreate,
                existingPublicationRoles: existingPublicationRoles,
                existingReleaseRoles: existingReleaseRoles
            );

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
        Assert.Equal(expectedNewSystemPublicationRoleToCreate, newSystemPublicationRoleToCreate);
    }

    [Theory]
    [MemberData(nameof(PublicationRoleRemovalData))]
    public void DetermineNewPermissionsSystemChangesForRoleRemoval_ForPublicationRole(
        PublicationRole oldPublicationRoleToRemove,
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove,
        PublicationRole? expectedNewSystemPublicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleRemoval(
                oldPublicationRoleToRemove: oldPublicationRoleToRemove,
                existingPublicationRoles: existingPublicationRoles,
                existingReleaseRoles: existingReleaseRoles
            );

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
        Assert.Equal(expectedNewSystemPublicationRoleToCreate, newSystemPublicationRoleToCreate);
    }

    [Theory]
    [InlineData(PublicationRole.Owner)]
    [InlineData(PublicationRole.Allower)]
    public void DetermineNewPermissionsSystemChangesForRoleRemoval_ForOldPublicationRoleWhichDoesNotExist_Throws(
        PublicationRole publicationRole
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var exception = Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleRemoval(
                oldPublicationRoleToRemove: publicationRole,
                existingPublicationRoles: [],
                existingReleaseRoles: []
            )
        );

        Assert.Equal(
            $"The publication role '{publicationRole}' is not in the existing list of publication roles.",
            exception.Message
        );
    }

    [Theory]
    [InlineData(PublicationRole.Drafter, false)]
    [InlineData(PublicationRole.Approver, false)]
    [InlineData(PublicationRole.Drafter, true)]
    [InlineData(PublicationRole.Approver, true)]
    public void DetermineNewPermissionsSystemChangesForRoleRemoval_ForNewPublicationRole_Throws(
        PublicationRole publicationRole,
        bool roleExists
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var exception = Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleRemoval(
                oldPublicationRoleToRemove: publicationRole,
                existingPublicationRoles: roleExists ? [publicationRole] : [],
                existingReleaseRoles: []
            )
        );

        Assert.Equal(
            $"Unexpected publication role: '{publicationRole}'. Expected an OLD permissions system role.",
            exception.Message
        );
    }

    [Theory]
    [MemberData(nameof(ReleaseRoleRemovalData))]
    public void DetermineNewPermissionsSystemChangesForRoleRemoval_ForReleaseRole(
        ReleaseRole releaseRoleToRemove,
        HashSet<PublicationRole> existingPublicationRoles,
        HashSet<ReleaseRole> existingReleaseRoles,
        PublicationRole? expectedNewSystemPublicationRoleToRemove,
        PublicationRole? expectedNewSystemPublicationRoleToCreate
    )
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var (newSystemPublicationRoleToRemove, newSystemPublicationRoleToCreate) =
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleRemoval(
                releaseRoleToRemove: releaseRoleToRemove,
                existingPublicationRoles: existingPublicationRoles,
                existingReleaseRoles: existingReleaseRoles
            );

        Assert.Equal(expectedNewSystemPublicationRoleToRemove, newSystemPublicationRoleToRemove);
        Assert.Equal(expectedNewSystemPublicationRoleToCreate, newSystemPublicationRoleToCreate);
    }

    [Fact]
    public void DetermineNewPermissionsSystemChangesForRoleRemoval_ForReleaseRoleWhichDoesNotExist_Throws()
    {
        var newPermissionsSystemHelper = SetupNewPermissionsSystemHelper();

        var exception = Assert.Throws<ArgumentException>(() =>
            newPermissionsSystemHelper.DetermineNewPermissionsSystemChangesForRoleRemoval(
                releaseRoleToRemove: ReleaseRole.Contributor,
                existingPublicationRoles: [],
                existingReleaseRoles: []
            )
        );

        Assert.Equal(
            $"The release role '{ReleaseRole.Contributor}' is not in the existing list of release roles.",
            exception.Message
        );
    }

    private static NewPermissionsSystemHelper SetupNewPermissionsSystemHelper()
    {
        return new NewPermissionsSystemHelper();
    }
}
