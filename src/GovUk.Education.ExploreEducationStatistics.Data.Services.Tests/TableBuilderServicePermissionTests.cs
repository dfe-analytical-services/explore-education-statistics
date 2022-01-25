#nullable enable
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
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TableBuilderServicePermissionTests
    {
        private static readonly Guid SubjectId = Guid.NewGuid();
        private static readonly Guid ReleaseId = Guid.NewGuid();

        private readonly Subject _subject = new()
        {
            Id = SubjectId,
        };

        private readonly Release _release = new()
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
            IFilterItemRepository? filterItemRepository = null,
            IObservationService? observationService = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            IResultSubjectMetaService? resultSubjectMetaService = null,
            ISubjectRepository? subjectRepository = null,
            IUserService? userService = null,
            IResultBuilder<Observation, ObservationViewModel>? resultBuilder = null,
            IReleaseRepository? releaseRepository = null,
            IOptions<TableBuilderOptions>? options = null)
        {
            return new(
                Mock.Of<StatisticsDbContext>(),
                filterItemRepository ?? Mock.Of<IFilterItemRepository>(MockBehavior.Strict),
                observationService ?? Mock.Of<IObservationService>(MockBehavior.Strict),
                statisticsPersistenceHelper ?? StatisticsPersistenceHelperMock(_subject).Object,
                resultSubjectMetaService ?? Mock.Of<IResultSubjectMetaService>(MockBehavior.Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(MockBehavior.Strict),
                userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
                resultBuilder ?? new ResultBuilder(DataServiceMapperUtils.DataServiceMapper()),
                releaseRepository ?? Mock.Of<IReleaseRepository>(MockBehavior.Strict),
                options ?? Options.Create(new TableBuilderOptions())
            );
        }

        private Mock<IPersistenceHelper<StatisticsDbContext>> StatisticsPersistenceHelperMock(Subject subject)
        {
            return MockUtils.MockPersistenceHelper<StatisticsDbContext, Subject>(subject.Id, subject);
        }
    }
}
