#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent;

public class ReleaseNoteServicePermissionTests
{
    private readonly ReleaseVersion _releaseVersion = new()
    {
        Id = Guid.NewGuid()
    };

    [Fact]
    public void AddReleaseNoteAsync()
    {
        AssertSecurityPoliciesChecked(service =>
                service.AddReleaseNoteAsync(
                    _releaseVersion.Id,
                    new ReleaseNoteSaveRequest()),
            SecurityPolicies.CanUpdateSpecificReleaseVersion);
    }

    [Fact]
    public void DeleteReleaseNoteAsync()
    {
        AssertSecurityPoliciesChecked(service =>
                service.DeleteReleaseNoteAsync(
                    releaseVersionId: _releaseVersion.Id,
                    releaseNoteId: Guid.NewGuid()),
            SecurityPolicies.CanUpdateSpecificReleaseVersion);
    }

    [Fact]
    public void UpdateReleaseNoteAsync()
    {
        AssertSecurityPoliciesChecked(service =>
                service.UpdateReleaseNoteAsync(
                    releaseVersionId: _releaseVersion.Id,
                    releaseNoteId: Guid.NewGuid(),
                    new ReleaseNoteSaveRequest()),
            SecurityPolicies.CanUpdateSpecificReleaseVersion);
    }

    private void AssertSecurityPoliciesChecked<T>(
        Func<ReleaseNoteService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
    {
        var (mapper, contentDbContext, releaseHelper, userService) = Mocks();

        var service = new ReleaseNoteService(mapper.Object, contentDbContext.Object, releaseHelper.Object, userService.Object);

        PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, _releaseVersion, userService, service, policies);
    }

    private (
        Mock<IMapper>,
        Mock<ContentDbContext>,
        Mock<IPersistenceHelper<ContentDbContext>>,
        Mock<IUserService>) Mocks()
    {
        return (
            new Mock<IMapper>(),
            new Mock<ContentDbContext>(),
            MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>(_releaseVersion.Id, _releaseVersion),
            new Mock<IUserService>());
    }
}
