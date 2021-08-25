#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
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
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.GetRelease(_release.Id);
                    }
                );
        }

        [Fact]
        public async Task GetReleaseStatuses()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanViewReleaseStatusHistory)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.GetReleaseStatuses(_release.Id);
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
                        var service = BuildReleaseService(userService: userService.Object);
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
        public async Task UpdateRelease()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.UpdateRelease(
                            _release.Id,
                            new ReleaseUpdateViewModel()
                        );
                    }
                );
        }

        [Fact]
        public async Task UpdateReleaseStatus_Draft()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanMarkSpecificReleaseAsDraft)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.CreateReleaseStatus(
                            _release.Id,
                            new ReleaseStatusCreateViewModel
                            {
                                ApprovalStatus = ReleaseApprovalStatus.Draft
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task UpdateReleaseStatus_HigherLevelReview()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanSubmitSpecificReleaseToHigherReview)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.CreateReleaseStatus(
                            _release.Id,
                            new ReleaseStatusCreateViewModel
                            {
                                ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task UpdateReleaseStatus_Approve()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanApproveSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.CreateReleaseStatus(
                            _release.Id,
                            new ReleaseStatusCreateViewModel
                            {
                                ApprovalStatus = ReleaseApprovalStatus.Approved
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
                        var service = BuildReleaseService(userService: userService.Object);
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
                        using (var contentDbContext = InMemoryApplicationDbContext("CreateReleaseAmendmentAsync"))
                        {
                            contentDbContext.Attach(_release);
                            var service = BuildReleaseService(contentDbContext,
                                userService: userService.Object);
                            return service.CreateReleaseAmendment(_release.Id);
                        }
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
                        var service = BuildReleaseService(userService: userService.Object);
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
                            userService: userService.Object,
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
                            userService: userService.Object,
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
                        var service = BuildReleaseService(userService: userService.Object);
                        return await service.RemoveDataFiles(_release.Id, Guid.NewGuid());
                    }
                );
        }

        private ReleaseService BuildReleaseService(
            ContentDbContext? context = null,
            IMapper? mapper = null,
            IPublishingService? publishingService = null,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null,
            IReleaseRepository? releaseRepository = null,
            IReleaseFileRepository? releaseFileRepository = null,
            ISubjectRepository? subjectRepository = null,
            IReleaseFileService? releaseFileService = null,
            IReleaseDataFileService? releaseDataFileService = null,
            IDataImportService? dataImportService = null,
            IFootnoteService? footnoteService = null,
            StatisticsDbContext? statisticsDbContext = null,
            IDataBlockService? dataBlockService = null,
            IReleaseChecklistService? releaseChecklistService = null,
            IContentBlockRepository? contentBlockRepository = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null)
        {
            return new(
                context ?? Mock.Of<ContentDbContext>(Strict),
                mapper ?? AdminMapper(),
                publishingService ?? Mock.Of<IPublishingService>(Strict),
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                userService ?? Mock.Of<IUserService>(Strict),
                releaseRepository ?? Mock.Of<IReleaseRepository>(Strict),
                releaseFileRepository ?? Mock.Of<IReleaseFileRepository>(Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(Strict),
                releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                dataImportService ?? Mock.Of<IDataImportService>(Strict),
                footnoteService ?? Mock.Of<IFootnoteService>(Strict),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(Strict),
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
                releaseChecklistService ?? Mock.Of<IReleaseChecklistService>(Strict),
                contentBlockRepository ?? Mock.Of<IContentBlockRepository>(Strict),
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
                new SequentialGuidGenerator()
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
