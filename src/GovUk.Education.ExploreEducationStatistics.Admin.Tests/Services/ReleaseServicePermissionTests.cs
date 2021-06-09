using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServicePermissionTests
    {
        private static readonly Publication Publication = new Publication
        {
            Id = Guid.NewGuid()
        };

        private readonly Release _release = new Release
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
        public async Task CreateReleaseAsync()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(Publication, CanCreateReleaseForSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.CreateReleaseAsync(
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
        public async Task UpdateRelease_Draft()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanMarkSpecificReleaseAsDraft)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.UpdateRelease(
                            _release.Id,
                            new ReleaseUpdateViewModel
                            {
                                Status = ReleaseStatus.Draft
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task UpdateRelease_SubmitForHigherLevelReview()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanSubmitSpecificReleaseToHigherReview)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.UpdateRelease(
                            _release.Id,
                            new ReleaseUpdateViewModel
                            {
                                Status = ReleaseStatus.HigherLevelReview
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task UpdateRelease_Approve()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(_release, CanUpdateSpecificRelease)
                .SetupResourceCheckToFail(_release, CanApproveSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.UpdateRelease(
                            _release.Id,
                            new ReleaseUpdateViewModel
                            {
                                Status = ReleaseStatus.Approved
                            }
                        );
                    }
                );
        }

        [Fact]
        public async Task GetLatestReleaseAsync()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(Publication, CanViewSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.GetLatestReleaseAsync(Publication.Id);
                    }
                );
        }

        [Fact]
        public async Task CreateReleaseAmendmentAsync()
        {
            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(_release, CanMakeAmendmentOfSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.CreateReleaseAmendmentAsync(_release.Id);
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
                new MyReleaseViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            repository
                .Setup(s => s.GetAllReleasesForReleaseStatusesAsync(ReleaseStatus.Approved))
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
                        var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);

                        Assert.Equal(list, result.Right);

                        return result;
                    }
                );

            repository.Verify(s => s.GetAllReleasesForReleaseStatusesAsync(ReleaseStatus.Approved));
            repository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetMyReleasesForReleaseStatusesAsync_CanViewRelatedReleases()
        {
            var repository = new Mock<IReleaseRepository>();

            var list = new List<MyReleaseViewModel>
            {
                new MyReleaseViewModel
                {
                    Id = Guid.NewGuid()
                }
            };

            repository
                .Setup(s => s.GetReleasesForReleaseStatusRelatedToUserAsync(_userId, ReleaseStatus.Approved))
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
                        var result = await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);

                        Assert.Equal(list, result.Right);

                        return result;
                    }
                );

            repository.Verify(s => s.GetReleasesForReleaseStatusRelatedToUserAsync(_userId, ReleaseStatus.Approved));
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
                        return await service.GetMyReleasesForReleaseStatusesAsync(ReleaseStatus.Approved);
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
            ContentDbContext context = null,
            IMapper mapper = null,
            IPublishingService publishingService = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IUserService userService = null,
            IReleaseRepository releaseRepository = null,
            IReleaseFileRepository releaseFileRepository = null,
            ISubjectService subjectService = null,
            IReleaseFileService releaseFileService = null,
            IReleaseDataFileService releaseDataFileService = null,
            IDataImportService dataImportService = null,
            IFootnoteService footnoteService = null,
            StatisticsDbContext statisticsDbContext = null,
            IDataBlockService dataBlockService = null,
            IReleaseChecklistService releaseChecklistService = null,
            IContentService contentService = null,
            IReleaseSubjectService releaseSubjectService = null)
        {
            return new ReleaseService(
                context ?? new Mock<ContentDbContext>().Object,
                mapper ?? AdminMapper(),
                publishingService ?? new Mock<IPublishingService>().Object,
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                userService ?? new Mock<IUserService>().Object,
                releaseRepository ?? new Mock<IReleaseRepository>().Object,
                releaseFileRepository ?? new Mock<IReleaseFileRepository>().Object,
                subjectService ?? new Mock<ISubjectService>().Object,
                releaseDataFileService ?? new Mock<IReleaseDataFileService>().Object,
                releaseFileService ?? new Mock<IReleaseFileService>().Object,
                dataImportService ?? new Mock<IDataImportService>().Object,
                footnoteService ?? new Mock<IFootnoteService>().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                dataBlockService ?? new Mock<IDataBlockService>().Object,
                releaseChecklistService ?? new Mock<IReleaseChecklistService>().Object,
                contentService ?? new Mock<IContentService>().Object,
                releaseSubjectService ?? new Mock<IReleaseSubjectService>().Object,
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
