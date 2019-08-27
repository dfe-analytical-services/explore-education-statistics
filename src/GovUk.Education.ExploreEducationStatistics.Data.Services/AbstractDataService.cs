using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public abstract class AbstractDataService<TResult> : IDataService<TResult>
    {
        private readonly IObservationService _observationService;
        private readonly ISubjectService _subjectService;

        protected AbstractDataService(IObservationService observationService,
            ISubjectService subjectService)
        {
            _observationService = observationService;
            _subjectService = subjectService;
        }

        public abstract TResult Query(ObservationQueryContext queryContext);

        protected IEnumerable<Observation> GetObservations(ObservationQueryContext queryContext)
        {
            if (!_subjectService.IsSubjectForLatestRelease(queryContext.SubjectId))
            {
                throw new InvalidOperationException("Subject is not for the latest release of this publication");
            }

            return _observationService.FindObservations(queryContext);
        }
    }
}