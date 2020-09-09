using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;

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
        public void GetRelease()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanViewSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.GetRelease(_release.Id);
                    }
                );
        }

        [Fact]
        public void CreateReleaseAsync()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(Publication, CanCreateReleaseForSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.CreateReleaseAsync(
                            new CreateReleaseViewModel
                            {
                                PublicationId = Publication.Id,
                            }
                        );
                    }
                );
        }

        [Fact]
        public void UpdateRelease()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.UpdateRelease(
                            _release.Id,
                            new UpdateReleaseViewModel
                            {
                                PublicationId = Publication.Id,
                            }
                        );
                    }
                );
        }

        [Fact]
        public void UpdateRelease_Draft()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheck(_release, CanUpdateSpecificRelease)
                .ExpectResourceCheckToFail(_release, CanMarkSpecificReleaseAsDraft)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.UpdateRelease(
                            _release.Id,
                            new UpdateReleaseViewModel
                            {
                                PublicationId = Publication.Id,
                                Status = ReleaseStatus.Draft
                            }
                        );
                    }
                );
        }

        [Fact]
        public void UpdateRelease_SubmitForHigherLevelReview()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheck(_release, CanUpdateSpecificRelease)
                .ExpectResourceCheckToFail(_release, CanSubmitSpecificReleaseToHigherReview)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.UpdateRelease(
                            _release.Id,
                            new UpdateReleaseViewModel
                            {
                                PublicationId = Publication.Id,
                                Status = ReleaseStatus.HigherLevelReview
                            }
                        );
                    }
                );
        }

        [Fact]
        public void UpdateRelease_Approve()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheck(_release, CanUpdateSpecificRelease)
                .ExpectResourceCheckToFail(_release, CanApproveSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.UpdateRelease(
                            _release.Id,
                            new UpdateReleaseViewModel
                            {
                                PublicationId = Publication.Id,
                                Status = ReleaseStatus.Approved
                            }
                        );
                    }
                );
        }

        [Fact]
        public void UpdateRelease_CanCreateReleaseForSpecificPublication()
        {
            var nextPublication = new Publication
            {
                Id = Guid.NewGuid(),
            };

            var persistenceHelper = DefaultPersistenceHelperMock();

            MockUtils.SetupCall(persistenceHelper, nextPublication.Id, nextPublication);

            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheck(_release, CanUpdateSpecificRelease)
                .ExpectResourceCheck(_release, CanMarkSpecificReleaseAsDraft)
                .ExpectResourceCheckToFail(nextPublication, CanCreateReleaseForSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(
                            persistenceHelper: persistenceHelper.Object,
                            userService: userService.Object
                        );
                        return service.UpdateRelease(
                            _release.Id,
                            new UpdateReleaseViewModel
                            {
                                PublicationId = nextPublication.Id,
                                Status = ReleaseStatus.Draft,
                            }
                        );
                    }
                );
        }

        [Fact]
        public void GetLatestReleaseAsync()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(Publication, CanViewSpecificPublication)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.GetLatestReleaseAsync(Publication.Id);
                    }
                );
        }

        [Fact]
        public void CreateReleaseAmendmentAsync()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanMakeAmendmentOfSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.CreateReleaseAmendmentAsync(_release.Id);
                    }
                );
        }

        [Fact]
        public void DeleteReleaseAsync()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanDeleteSpecificRelease)
                .AssertForbidden(
                    userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return service.DeleteReleaseAsync(_release.Id);
                    }
                );
        }

        [Fact]
        public void GetMyReleasesForReleaseStatusesAsync_CanViewAllReleases()
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

            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectCheck(CanAccessSystem)
                .ExpectCheck(CanViewAllReleases)
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
        public void GetMyReleasesForReleaseStatusesAsync_CanViewRelatedReleases()
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

            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectCheck(CanAccessSystem)
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
        public void GetMyReleasesForReleaseStatusesAsync_NoAccessToSystem()
        {
            PermissionTestUtil.PolicyCheckBuilder()
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
        public void DeleteDataFilesAsync()
        {
            PermissionTestUtil.PolicyCheckBuilder()
                .ExpectResourceCheckToFail(_release, CanUpdateSpecificRelease)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = BuildReleaseService(userService: userService.Object);
                        return await service.RemoveDataFilesAsync(_release.Id, "", "");
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
            ISubjectService subjectService = null,
            ITableStorageService tableStorageService = null,
            IReleaseFilesService releaseFilesService = null,
            IImportStatusService importStatusService = null,
            IFootnoteService footnoteService = null,
            StatisticsDbContext statisticsDbContext = null,
            IDataBlockService dataBlockService = null,
            IReleaseSubjectService releaseSubjectService = null)
        {
            return new ReleaseService(
                context ?? new Mock<ContentDbContext>().Object,
                mapper ?? AdminMapper(),
                publishingService ?? new Mock<IPublishingService>().Object,
                persistenceHelper ?? DefaultPersistenceHelperMock().Object,
                userService ?? new Mock<IUserService>().Object,
                releaseRepository ?? new Mock<IReleaseRepository>().Object,
                subjectService ?? new Mock<ISubjectService>().Object,
                tableStorageService ?? new Mock<ITableStorageService>().Object,
                releaseFilesService ?? new Mock<IReleaseFilesService>().Object,
                importStatusService ?? new Mock<IImportStatusService>().Object,
                footnoteService ?? new Mock<IFootnoteService>().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                dataBlockService ?? new Mock<IDataBlockService>().Object,
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