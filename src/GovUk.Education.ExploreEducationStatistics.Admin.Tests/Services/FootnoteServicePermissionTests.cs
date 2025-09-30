#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FootnoteServicePermissionTests
{
    private static readonly ReleaseVersion ReleaseVersion = new() { Id = Guid.NewGuid() };

    private static readonly Subject Subject = new() { Id = Guid.NewGuid() };

    private static readonly Footnote Footnote = new()
    {
        Id = Guid.NewGuid(),
        Subjects = new List<SubjectFootnote> { new() { SubjectId = Subject.Id } },
    };

    [Fact]
    public async Task CreateFootnote()
    {
        await AssertSecurityPolicyChecked(
            service =>
                service.CreateFootnote(
                    ReleaseVersion.Id,
                    "",
                    filterIds: SetOf<Guid>(),
                    filterGroupIds: SetOf<Guid>(),
                    filterItemIds: SetOf<Guid>(),
                    indicatorIds: SetOf<Guid>(),
                    subjectIds: SetOf(Subject.Id)
                ),
            ReleaseVersion,
            SecurityPolicies.CanUpdateSpecificReleaseVersion
        );
    }

    [Fact]
    public async Task DeleteFootnote()
    {
        await AssertSecurityPolicyChecked(
            service =>
                service.DeleteFootnote(
                    releaseVersionId: ReleaseVersion.Id,
                    footnoteId: Footnote.Id
                ),
            ReleaseVersion,
            SecurityPolicies.CanUpdateSpecificReleaseVersion
        );
    }

    [Fact]
    public async Task GetFootnote()
    {
        await AssertSecurityPolicyChecked(
            service =>
                service.GetFootnote(releaseVersionId: ReleaseVersion.Id, footnoteId: Footnote.Id),
            ReleaseVersion,
            ContentSecurityPolicies.CanViewSpecificReleaseVersion
        );
    }

    [Fact]
    public async Task GetFootnotes()
    {
        await AssertSecurityPolicyChecked(
            service => service.GetFootnotes(ReleaseVersion.Id),
            ReleaseVersion,
            ContentSecurityPolicies.CanViewSpecificReleaseVersion
        );
    }

    [Fact]
    public async Task UpdateFootnote()
    {
        await AssertSecurityPolicyChecked(
            service =>
                service.UpdateFootnote(
                    releaseVersionId: ReleaseVersion.Id,
                    footnoteId: Footnote.Id,
                    "",
                    filterIds: SetOf<Guid>(),
                    filterGroupIds: SetOf<Guid>(),
                    filterItemIds: SetOf<Guid>(),
                    indicatorIds: SetOf<Guid>(),
                    subjectIds: SetOf(Subject.Id)
                ),
            ReleaseVersion,
            SecurityPolicies.CanUpdateSpecificReleaseVersion
        );
    }

    [Fact]
    public async Task UpdateFootnotes()
    {
        await AssertSecurityPolicyChecked(
            service =>
                service.UpdateFootnotes(
                    ReleaseVersion.Id,
                    new FootnotesUpdateRequest { FootnoteIds = new List<Guid>() }
                ),
            ReleaseVersion,
            SecurityPolicies.CanUpdateSpecificReleaseVersion
        );
    }

    private static Task AssertSecurityPolicyChecked<T, TResource, TPolicy>(
        Func<FootnoteService, Task<Either<ActionResult, T>>> protectedAction,
        TResource resource,
        TPolicy policy
    )
        where TPolicy : Enum
    {
        var (
            contentPersistenceHelper,
            dataBlockService,
            footnoteService,
            statisticsPersistenceHelper,
            releaseSubjectRepository
        ) = Mocks();

        return PermissionTestUtils
            .PolicyCheckBuilder<TPolicy>()
            .SetupResourceCheck(resource, policy, false)
            .AssertForbidden(async userService =>
            {
                var service = new FootnoteService(
                    InMemoryStatisticsDbContext(),
                    contentPersistenceHelper.Object,
                    userService.Object,
                    dataBlockService.Object,
                    footnoteService.Object,
                    releaseSubjectRepository.Object,
                    statisticsPersistenceHelper.Object
                );

                return await protectedAction.Invoke(service);
            });
    }

    private static (
        Mock<IPersistenceHelper<ContentDbContext>>,
        Mock<IDataBlockService>,
        Mock<IFootnoteRepository>,
        Mock<IPersistenceHelper<StatisticsDbContext>>,
        Mock<IReleaseSubjectRepository>
    ) Mocks()
    {
        return (
            MockPersistenceHelper<ContentDbContext, ReleaseVersion>(
                ReleaseVersion.Id,
                ReleaseVersion
            ),
            new Mock<IDataBlockService>(),
            new Mock<IFootnoteRepository>(),
            MockPersistenceHelper<StatisticsDbContext, Footnote>(Footnote.Id, Footnote),
            new Mock<IReleaseSubjectRepository>()
        );
    }
}
