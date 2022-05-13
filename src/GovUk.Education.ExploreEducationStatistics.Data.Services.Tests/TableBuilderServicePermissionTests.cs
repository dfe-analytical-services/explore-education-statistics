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
            Id = Guid.NewGuid(),
        };

        private readonly Release _release = new()
        {
            Id = Guid.NewGuid(),
            PublicationId = Guid.NewGuid()
        };

        private readonly ReleaseSubject _releaseSubject = new()
        {
            ReleaseId = ReleaseId,
            SubjectId = SubjectId,
        };

        [Fact]
        public async Task Query_LatestRelease_CanViewSubjectData()
        {
            await PolicyCheckBuilder<DataSecurityPolicies>()
                .SetupResourceCheckToFail(_releaseSubject, DataSecurityPolicies.CanViewSubjectData)
                .AssertForbidden(
                    async userService =>
                    {
                        var statisticsPersistenceHelper = StatisticsPersistenceHelperMock(_subject);

                        MockUtils.SetupCall(statisticsPersistenceHelper, _releaseSubject);

                        var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);

                        subjectRepository
                            .Setup(s => s.GetPublicationIdForSubject(_subject.Id))
                            .ReturnsAsync(_release.PublicationId);

                        var releaseRepository = new Mock<IReleaseRepository>(MockBehavior.Strict);

                        releaseRepository
                            .Setup(s => s.GetLatestPublishedRelease(_release.PublicationId))
                            .Returns(_release);

                        var service = BuildTableBuilderService(
                            userService: userService.Object,
                            subjectRepository: subjectRepository.Object,
                            releaseRepository: releaseRepository.Object,
                            statisticsPersistenceHelper: statisticsPersistenceHelper.Object
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
                .SetupResourceCheckToFail(_releaseSubject, DataSecurityPolicies.CanViewSubjectData)
                .AssertForbidden(
                    async userService =>
                    {
                        var statisticsPersistenceHelper = StatisticsPersistenceHelperMock(_subject);

                        MockUtils.SetupCall(statisticsPersistenceHelper, _releaseSubject);

                        var service = BuildTableBuilderService(
                            userService: userService.Object,
                            statisticsPersistenceHelper: statisticsPersistenceHelper.Object
                        );
                        return await service.Query(
                            _release.Id,
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
                resultBuilder ?? new ResultBuilder(),
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
