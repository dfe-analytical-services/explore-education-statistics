using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TableBuilderServicePermissionTests
    {
        private static readonly Guid SubjectId = Guid.NewGuid();
        private static readonly Guid ReleaseId = Guid.NewGuid();

        private readonly Subject _subject = new Subject
        {
            Id = SubjectId,
        };

        private readonly Release _release = new Release
        {
            Id = ReleaseId,
        };

        [Fact]
        public void Query_LatestRelease_CanViewSubjectData()
        {
            PermissionTestUtils.PolicyCheckBuilder<DataSecurityPolicies>()
                .ExpectResourceCheckToFail(_subject, DataSecurityPolicies.CanViewSubjectData)
                .AssertForbidden(
                    async userService =>
                    {
                        var publication = new Model.Publication
                        {
                            Id = Guid.NewGuid(),
                        };

                        var release = new Model.Release
                        {
                            Id = Guid.NewGuid(),
                        };

                        var subjectService = new Mock<ISubjectService>();

                        subjectService
                            .Setup(s => s.IsSubjectForLatestPublishedRelease(_subject.Id))
                            .ReturnsAsync(false);

                        subjectService
                            .Setup(s => s.GetPublicationForSubject(_subject.Id))
                            .ReturnsAsync(publication);

                        var releaseService = new Mock<IReleaseRepository>();

                        releaseService
                            .Setup(s => s.GetLatestPublishedRelease(publication.Id))
                            .Returns(release);

                        var service = BuildTableBuilderService(
                            userService: userService.Object,
                            subjectService: subjectService.Object,
                            releaseRepository: releaseService.Object
                        );

                        return await service.Query(
                            new ObservationQueryContext
                            {
                                SubjectId = _subject.Id
                            }
                        );
                    }
                );
        }

        [Fact]
        public void Query_ReleaseId_CanViewSubjectData()
        {
            PermissionTestUtils.PolicyCheckBuilder<DataSecurityPolicies>()
                .ExpectResourceCheckToFail(_subject, DataSecurityPolicies.CanViewSubjectData)
                .AssertForbidden(
                    async userService =>
                    {
                        var releaseSubject = new ReleaseSubject
                        {
                            ReleaseId = _release.Id,
                            Release = _release,
                            SubjectId = _subject.Id,
                            Subject = _subject,
                        };

                        var statisticsPersistenceHelper = StatisticsPersistenceHelperMock(_subject);

                        MockUtils.SetupCall(statisticsPersistenceHelper, releaseSubject);

                        var subjectService = new Mock<ISubjectService>();

                        subjectService
                            .Setup(s => s.IsSubjectForLatestPublishedRelease(_subject.Id))
                            .ReturnsAsync(false);

                        var service = BuildTableBuilderService(
                            userService: userService.Object,
                            subjectService: subjectService.Object,
                            statisticsPersistenceHelper: statisticsPersistenceHelper.Object
                        );
                        return await service.Query(
                            releaseSubject.ReleaseId,
                            new ObservationQueryContext
                            {
                                SubjectId = _subject.Id
                            }
                        );
                    }
                );
        }

        private TableBuilderService BuildTableBuilderService(
            IObservationService observationService = null,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper = null,
            IResultSubjectMetaService resultSubjectMetaService = null,
            ISubjectService subjectService = null,
            IUserService userService = null,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder = null,
            IReleaseRepository releaseRepository = null)
        {
            return new TableBuilderService(
                observationService ?? new Mock<IObservationService>().Object,
                statisticsPersistenceHelper ?? StatisticsPersistenceHelperMock(_subject).Object,
                resultSubjectMetaService ?? new Mock<IResultSubjectMetaService>().Object,
                subjectService ?? new Mock<ISubjectService>().Object,
                userService ?? new Mock<IUserService>().Object,
                resultBuilder ?? new ResultBuilder(DataServiceMapperUtils.DataServiceMapper()),
                releaseRepository ?? new Mock<IReleaseRepository>().Object
            );
        }

        private Mock<IPersistenceHelper<StatisticsDbContext>> StatisticsPersistenceHelperMock(Subject subject)
        {
            return MockUtils.MockPersistenceHelper<StatisticsDbContext, Subject>(subject.Id, subject);
        }
    }
}