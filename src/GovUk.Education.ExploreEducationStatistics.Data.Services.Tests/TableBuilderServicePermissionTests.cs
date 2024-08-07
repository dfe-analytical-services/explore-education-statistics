#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TableBuilderServicePermissionTests
    {
        private static readonly Guid PublicationId = Guid.NewGuid();
        private static readonly Guid ReleaseVersionId = Guid.NewGuid();
        private static readonly Guid SubjectId = Guid.NewGuid();

        private readonly Subject _subject = new()
        {
            Id = Guid.NewGuid(),
        };

        private readonly ReleaseSubject _releaseSubject = new()
        {
            ReleaseVersionId = ReleaseVersionId,
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
                            .Setup(s => s.FindPublicationIdForSubject(_subject.Id, default))
                            .ReturnsAsync(PublicationId);

                        var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);

                        releaseVersionRepository
                            .Setup(s => s.GetLatestPublishedReleaseVersion(PublicationId, default))
                            .ReturnsAsync(new ReleaseVersion
                            {
                                Id = ReleaseVersionId
                            });

                        var service = BuildTableBuilderService(
                            userService: userService.Object,
                            subjectRepository: subjectRepository.Object,
                            releaseVersionRepository: releaseVersionRepository.Object,
                            statisticsPersistenceHelper: statisticsPersistenceHelper.Object
                        );

                        return await service.Query(
                            new FullTableQuery
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
                            ReleaseVersionId,
                            new FullTableQuery
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
            ISubjectResultMetaService? subjectResultMetaService = null,
            ISubjectCsvMetaService? subjectCsvMetaService = null,
            ISubjectRepository? subjectRepository = null,
            IUserService? userService = null,
            IReleaseVersionRepository? releaseVersionRepository = null,
            IOptions<TableBuilderOptions>? options = null)
        {
            return new(
                Mock.Of<StatisticsDbContext>(),
                filterItemRepository ?? Mock.Of<IFilterItemRepository>(MockBehavior.Strict),
                observationService ?? Mock.Of<IObservationService>(MockBehavior.Strict),
                statisticsPersistenceHelper ?? StatisticsPersistenceHelperMock(_subject).Object,
                subjectResultMetaService ?? Mock.Of<ISubjectResultMetaService>(MockBehavior.Strict),
                subjectCsvMetaService ?? Mock.Of<ISubjectCsvMetaService>(MockBehavior.Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(MockBehavior.Strict),
                userService ?? Mock.Of<IUserService>(MockBehavior.Strict),
                releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict),
                options ?? Options.Create(new TableBuilderOptions())
            );
        }

        private Mock<IPersistenceHelper<StatisticsDbContext>> StatisticsPersistenceHelperMock(Subject subject)
        {
            return MockUtils.MockPersistenceHelper<StatisticsDbContext, Subject>(subject.Id, subject);
        }
    }
}
