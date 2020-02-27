using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public abstract class AbstractDataService<TResult> : IDataService<TResult>
    {
        private readonly IObservationService _observationService;
        protected readonly IUserService _userService;
        protected readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;

        protected AbstractDataService(IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            IUserService userService)
        {
            _observationService = observationService;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
        }

        public abstract Task<Either<ActionResult, TResult>> Query(ObservationQueryContext queryContext);

        protected IEnumerable<Observation> GetObservations(ObservationQueryContext queryContext)
        {
            return _observationService.FindObservations(queryContext);
        }
    }
}