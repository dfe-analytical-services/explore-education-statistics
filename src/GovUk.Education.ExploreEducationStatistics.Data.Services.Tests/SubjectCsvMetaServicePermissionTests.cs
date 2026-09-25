#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class SubjectCsvMetaServicePermissionTests
{
    private static readonly Guid ReleaseVersionId = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();

    private static readonly ReleaseSubject ReleaseSubject = new()
    {
        ReleaseVersionId = ReleaseVersionId,
        SubjectId = SubjectId,
    };

    [Fact]
    public async Task GetSubjectCsvMeta()
    {
        await PolicyCheckBuilder<DataSecurityPolicies>()
            .SetupResourceCheckToFail(ReleaseSubject, DataSecurityPolicies.CanViewSubjectData)
            .AssertForbidden(async userService =>
            {
                var service = BuildService(userService.Object);

                var query = new FullTableQuery { SubjectId = SubjectId };

                return await service.GetSubjectCsvMeta(ReleaseSubject, query);
            });
    }

    private static SubjectCsvMetaService BuildService(
        IUserService userService,
        ContentDbContext? contentDbContext = null,
        IStorageDataSetResolver? storageDataSetResolver = null,
        IReleaseFileBlobService? releaseFileBlobService = null
    )
    {
        return new SubjectCsvMetaService(
            logger: Mock.Of<ILogger<SubjectCsvMetaService>>(),
            contentDbContext: contentDbContext ?? Mock.Of<ContentDbContext>(),
            userService: userService,
            storageDataSetResolver: storageDataSetResolver ?? Mock.Of<IStorageDataSetResolver>(Strict),
            releaseFileBlobService: releaseFileBlobService ?? Mock.Of<IReleaseFileBlobService>(Strict)
        );
    }
}
