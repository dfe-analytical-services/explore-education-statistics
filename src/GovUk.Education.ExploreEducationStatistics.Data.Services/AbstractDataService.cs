using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using ReleaseId = System.Guid;

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

        public abstract TResult Query(ObservationQueryContext queryContext, ReleaseId? releaseId = null);

        protected IEnumerable<Observation> GetObservations(ObservationQueryContext queryContext, ReleaseId? releaseId = null)
        {
            // If release Id is not specified then verify that the subject id passed exists for the latest release.
            if (releaseId == null && !_subjectService.IsSubjectForLatestRelease(queryContext.SubjectId))
            {
                throw new InvalidOperationException("Subject is not for the latest release of this publication");
            }

            return _observationService.FindObservations(queryContext);
        }
    }
}