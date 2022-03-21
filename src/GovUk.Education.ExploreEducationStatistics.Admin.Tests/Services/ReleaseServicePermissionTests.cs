#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServicePermissionTests
    {
        private static readonly Publication Publication = new()
        {
            Id = Guid.NewGuid()
        };

        private readonly Release _release = new()
        {
            Id = Guid.NewGuid(),
            PublicationId = Publication.Id,
            Published = DateTime.Now,
            TimePeriodCoverage = TimeIdentifier.April
        };

        private readonly Guid _userId = Guid.NewGuid();

        [Fact]
        public async Task GetRelease()
        {
            await PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheckToFail(_release, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.GetRelease(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task CreateRelease()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(Publication, CanCreateReleaseForSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.CreateRelease(
                            new ReleaseCreateViewModel
                            {
                                PublicationId = Publication.Id,
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task GetLatestPublishedRelease()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(Publication, CanViewSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.GetLatestPublishedRelease(Publication.Id);
                    }
                );
        }

        [Fact]
        public async Task CreateReleaseAmendment()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanMakeAmendmentOfSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        using var contentDbContext = InMemoryApplicationDbContext("CreateReleaseAmendmentAsync");
                        contentDbContext.Attach(_release);
                        var service = BuildReleaseService(
                            userService.Object,
                            contentDbContext);
                        return service.CreateReleaseAmendment(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task GetDeleteReleasePlan()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanDeleteSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.GetDeleteReleasePlan(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task DeleteRelease()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanDeleteSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.DeleteRelease(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task GetMyReleasesForReleaseStatusesAsync_CanViewAllReleases()
        {
            var repository = new Mock<IReleaseRepository>();

            var list = new List<MyReleaseViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid()
                }
            };

            repository
                .Setup(s => s.ListReleases(ReleaseApprovalStatus.Approved))
                .ReturnsAsync(list);

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupCheck(CanAccessSystem)
                .SetupCheck(CanViewAllReleases)
                .AssertSuccess(
                    async userService =>
                    {
                        var service = BuildReleaseService(
                            userService.Object,
                            releaseRepository: repository.Object
                        );
                        var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseApprovalStatus.Approved);

                        Assert.Equal(list, result.Right);

                        return result;
                    }
                );

            repository.Verify(s => s.ListReleases(ReleaseApprovalStatus.Approved));
            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetMyReleasesForReleaseStatusesAsync_CanViewRelatedReleases()
        {
            var repository = new Mock<IReleaseRepository>();

            var list = new List<MyReleaseViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid()
                }
            };

            repository
                .Setup(s => s.ListReleasesForUser(_userId, ReleaseApprovalStatus.Approved))
                .ReturnsAsync(list);

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupCheck(CanAccessSystem)
                .ExpectCheckToFail(CanViewAllReleases)
                .AssertSuccess(
                    async userService =>
                    {
                        userService
                            .Setup(s => s.GetUserId())
                            .Returns(_userId);

                        var service = BuildReleaseService(
                            userService.Object,
                            releaseRepository: repository.Object
                        );
                        var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseApprovalStatus.Approved);

                        Assert.Equal(list, result.Right);

                        return result;
                    }
                );

            repository.Verify(s => s.ListReleasesForUser(_userId, ReleaseApprovalStatus.Approved));
            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetMyReleasesForReleaseStatusesAsync_NoAccessToSystem()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(CanAccessSystem)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return await service.GetMyReleasesForReleaseStatusesAsync(ReleaseApprovalStatus.Approved);
                    }
                );
        }

        [Fact]
        public async Task RemoveDataFiles()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return await service.RemoveDataFiles(_release.Id, Guid.NewGuid());
                    }
                );
        }

        private ReleaseService BuildReleaseService(
            IUserService userService,
            ContentDbContext? context = null,
            IReleaseRepository? releaseRepository = null)
        {
            return new ReleaseService(
                context ?? Mock.Of<ContentDbContext>(),
                AdminMapper(),
                DefaultPersistenceHelperMock().Object,
                userService,
                releaseRepository ?? Mock.Of<IReleaseRepository>(),
                Mock.Of<IReleaseFileRepository>(),
                Mock.Of<ISubjectRepository>(),
                Mock.Of<IReleaseDataFileService>(),
                Mock.Of<IReleaseFileService>(),
                Mock.Of<IDataImportService>(),
                Mock.Of<IFootnoteService>(),
                Mock.Of<StatisticsDbContext>(),
                Mock.Of<IDataBlockService>(),
                Mock.Of<IReleaseSubjectRepository>(),
                new SequentialGuidGenerator(),
                Mock.Of<IBlobCacheService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, Release>();
            MockUtils.SetupCall(mock, _release.Id, _release);
            MockUtils.SetupCall(mock, Publication.Id, Publication);

            return mock;
        }
    }
}
