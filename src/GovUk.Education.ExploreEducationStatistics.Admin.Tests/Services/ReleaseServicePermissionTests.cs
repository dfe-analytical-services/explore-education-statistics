#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServicePermissionTests
    {
        private static readonly Publication Publication = new()
        {
            Id = Guid.NewGuid()
        };

        private readonly ReleaseVersion _releaseVersion = new()
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
                .SetupResourceCheckToFail(_releaseVersion, ContentSecurityPolicies.CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.GetRelease(_releaseVersion.Id);
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
                            new ReleaseCreateRequest
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
        public async Task GetDeleteReleaseVersionPlan()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanDeleteSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.GetDeleteReleaseVersionPlan(_releaseVersion.Id);
                    }
                );
        }

        [Fact]
        public async Task DeleteRelease()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanDeleteSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.DeleteReleaseVersion(_releaseVersion.Id);
                    }
                );
        }

        [Fact]
        public async Task ListReleasesWithStatuses_CanViewAllReleases()
        {
            var releaseVersionRepository = new Mock<IReleaseVersionRepository>();

            var list = new List<ReleaseVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ReleaseName = "2000",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                }
            };

            releaseVersionRepository
                .Setup(s => s.ListReleases(ReleaseApprovalStatus.Approved))
                .ReturnsAsync(list);

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupCheck(RegisteredUser)
                .SetupCheck(CanViewAllReleases)
                .AssertSuccess(
                    async userService =>
                    {
                        userService
                            .Setup(s => s.MatchesPolicy(list[0], CanAssignPreReleaseUsersToSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.MatchesPolicy(list[0], ContentSecurityPolicies.CanViewSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.MatchesPolicy(list[0], CanUpdateSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.MatchesPolicy(list[0], CanDeleteSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.MatchesPolicy(list[0], CanMakeAmendmentOfSpecificRelease))
                            .ReturnsAsync(true);

                        var service = BuildReleaseService(
                            userService.Object,
                            releaseVersionRepository: releaseVersionRepository.Object
                        );
                        var result = await service.ListReleasesWithStatuses(ReleaseApprovalStatus.Approved);

                        var viewModel = result.AssertRight();
                        Assert.Single(viewModel);
                        Assert.Equal(list[0].Id, viewModel[0].Id);

                        return result;
                    }
                );

            releaseVersionRepository.Verify(s => s.ListReleases(ReleaseApprovalStatus.Approved));
            releaseVersionRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ListReleasesWithStatuses_CanViewRelatedReleases()
        {
            var repository = new Mock<IReleaseVersionRepository>();

            var list = new List<ReleaseVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ReleaseName = "2000",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                }
            };

            repository
                .Setup(s => s.ListReleasesForUser(_userId, ReleaseApprovalStatus.Approved))
                .ReturnsAsync(list);

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupCheck(RegisteredUser)
                .ExpectCheckToFail(CanViewAllReleases)
                .AssertSuccess(
                    async userService =>
                    {
                        userService
                            .Setup(s => s.MatchesPolicy(list[0], CanAssignPreReleaseUsersToSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.MatchesPolicy(list[0], ContentSecurityPolicies.CanViewSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.MatchesPolicy(list[0], CanUpdateSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.MatchesPolicy(list[0], CanDeleteSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.MatchesPolicy(list[0], CanMakeAmendmentOfSpecificRelease))
                            .ReturnsAsync(true);

                        userService
                            .Setup(s => s.GetUserId())
                            .Returns(_userId);

                        var service = BuildReleaseService(
                            userService.Object,
                            releaseVersionRepository: repository.Object
                        );
                        var result = await service.ListReleasesWithStatuses(ReleaseApprovalStatus.Approved);

                        var viewModel = result.AssertRight();
                        Assert.Single(viewModel);
                        Assert.Equal(list[0].Id, viewModel[0].Id);

                        return result;
                    }
                );

            repository.Verify(s => s.ListReleasesForUser(_userId, ReleaseApprovalStatus.Approved));
            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ListReleasesWithStatuses_NoAccessToSystem()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(RegisteredUser)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return await service.ListReleasesWithStatuses(ReleaseApprovalStatus.Approved);
                    }
                );
        }

        [Fact]
        public async Task RemoveDataFiles()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_releaseVersion, CanUpdateSpecificRelease)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return await service.RemoveDataFiles(_releaseVersion.Id, Guid.NewGuid());
                    }
                );
        }

        [Fact]
        public async Task UpdateReleasePublished()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .ExpectCheckToFail(IsBauUser)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService.Object);
                        return service.UpdateReleasePublished(_releaseVersion.Id,
                            new ReleasePublishedUpdateRequest());
                    }
                );
        }

        private ReleaseService BuildReleaseService(
            IUserService userService,
            ContentDbContext? context = null,
            IReleaseVersionRepository? releaseVersionRepository = null)
        {
            return new ReleaseService(
                context ?? Mock.Of<ContentDbContext>(),
                Mock.Of<StatisticsDbContext>(),
                AdminMapper(),
                DefaultPersistenceHelperMock().Object,
                userService,
                releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(),
                Mock.Of<IReleaseCacheService>(),
                Mock.Of<IReleaseFileRepository>(),
                Mock.Of<IReleaseDataFileService>(),
                Mock.Of<IReleaseFileService>(),
                Mock.Of<IDataImportService>(),
                Mock.Of<IFootnoteRepository>(),
                Mock.Of<IDataBlockService>(),
                Mock.Of<IReleaseSubjectRepository>(),
                Mock.Of<IProcessorClient>(),
                new SequentialGuidGenerator(),
                Mock.Of<IBlobCacheService>()
            );
        }

        private Mock<IPersistenceHelper<ContentDbContext>> DefaultPersistenceHelperMock()
        {
            var mock = MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseVersion>();
            MockUtils.SetupCall(mock, _releaseVersion.Id, _releaseVersion);
            MockUtils.SetupCall(mock, Publication.Id, Publication);

            return mock;
        }
    }
}
