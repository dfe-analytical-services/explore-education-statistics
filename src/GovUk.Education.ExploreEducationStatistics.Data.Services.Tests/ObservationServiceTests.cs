#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ObservationService;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class ObservationServiceTests
    {
        [Fact]
        public async Task FindObservations_CancellationDuringStoredProcedure()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using var context = InMemoryStatisticsDbContext(statisticsDbContextId);
            var (service, matchingObservationsGetter) = BuildServiceAndMocks(context);
            var observationQueryContext = new ObservationQueryContext();
            
            matchingObservationsGetter
                .Setup(s => s
                    .GetMatchingObservationIdsQuery(context, observationQueryContext))
                .Callback(() => cancellationTokenSource.Cancel())
                .ReturnsAsync(AsArray(Guid.NewGuid()));

            await Assert.ThrowsAsync<OperationCanceledException>(() => service.FindObservations(observationQueryContext, cancellationToken));
        }
        
        private static (
            ObservationService service, 
            Mock<IMatchingObservationsGetter> matchingObservationsGetter
            ) BuildServiceAndMocks(StatisticsDbContext context)
        {
            var matchingObservationGetter = new Mock<IMatchingObservationsGetter>(Strict);
            var service = new ObservationService(context, new Mock<ILogger<ObservationService>>().Object)
            {
                MatchingObservationGetter = matchingObservationGetter.Object
            };
            return (service, matchingObservationGetter);
        }
    }
}