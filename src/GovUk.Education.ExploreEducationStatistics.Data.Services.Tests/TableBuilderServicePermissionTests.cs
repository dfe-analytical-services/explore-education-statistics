using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

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
        public async Task Query_LatestRelease_CanViewSubjectData()
        {
            await PolicyCheckBuilder<DataSecurityPolicies>()
                .SetupResourceCheckToFail(_subject, DataSecurityPolicies.CanViewSubjectData)
                .AssertForbidden(
                    async userService =>
                    {
                        var publicationId = Guid.NewGuid();

                        var release = new Release
                        {
                            Id = Guid.NewGuid(),
                            PublicationId = publicationId,
                        };

                        var subjectRepository = new Mock<ISubjectRepository>();

                        subjectRepository
                            .Setup(s => s.IsSubjectForLatestPublishedRelease(_subject.Id))
                            .ReturnsAsync(false);

                        subjectRepository
                            .Setup(s => s.GetPublicationIdForSubject(_subject.Id))
                            .ReturnsAsync(publicationId);

                        var releaseRepository = new Mock<IReleaseRepository>();

                        releaseRepository
                            .Setup(s => s.GetLatestPublishedRelease(publicationId))
                            .Returns(release);

                        var service = BuildTableBuilderService(
                            userService: userService.Object,
                            subjectRepository: subjectRepository.Object,
                            releaseRepository: releaseRepository.Object
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
        public async Task Query_ReleaseId_CanViewSubjectData()
        {
            await PolicyCheckBuilder<DataSecurityPolicies>()
                .SetupResourceCheckToFail(_subject, DataSecurityPolicies.CanViewSubjectData)
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

                        var subjectRepository = new Mock<ISubjectRepository>();

                        subjectRepository
                            .Setup(s => s.IsSubjectForLatestPublishedRelease(_subject.Id))
                            .ReturnsAsync(false);

                        var service = BuildTableBuilderService(
                            userService: userService.Object,
                            subjectRepository: subjectRepository.Object,
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
            ISubjectRepository subjectRepository = null,
            IUserService userService = null,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder = null,
            IReleaseRepository releaseRepository = null)
        {
            return new TableBuilderService(
                observationService ?? new Mock<IObservationService>().Object,
                statisticsPersistenceHelper ?? StatisticsPersistenceHelperMock(_subject).Object,
                resultSubjectMetaService ?? new Mock<IResultSubjectMetaService>().Object,
                subjectRepository ?? new Mock<ISubjectRepository>().Object,
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
