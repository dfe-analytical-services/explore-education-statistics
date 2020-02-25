using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public abstract class AbstractDataService<TResult> : IDataService<TResult>
    {
        private readonly IObservationService _observationService;
        private readonly ISubjectService _subjectService;
        protected readonly IUserService _userService;

        protected AbstractDataService(IObservationService observationService,
            ISubjectService subjectService,
            IUserService userService)
        {
            _observationService = observationService;
            _subjectService = subjectService;
            _userService = userService;
        }

        public abstract Task<Either<ActionResult, TResult>> Query(ObservationQueryContext queryContext);

        protected IEnumerable<Observation> GetObservations(ObservationQueryContext queryContext)
        {
            return _observationService.FindObservations(queryContext);
        }

        protected async Task<Either<ActionResult, Subject>> CheckSubjectExists(Guid subjectId)
        {
            var subject = _subjectService.Find(subjectId, new List<Expression<Func<Subject, object>>>
            {
                s => s.Release
            });
            return subject == null ? new NotFoundResult() : new Either<ActionResult, Subject>(subject);
        }
    }
}