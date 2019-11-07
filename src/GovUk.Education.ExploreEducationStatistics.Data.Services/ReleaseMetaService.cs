using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ReleaseMetaService : IReleaseMetaService
    {
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public ReleaseMetaService(
            ISubjectService subjectService,
            IMapper mapper)
        {
            _subjectService = subjectService;
            _mapper = mapper;
        }

        public IEnumerable<IdLabel> GetSubjects(Guid releaseId)
        {
            return _mapper.Map<IEnumerable<IdLabel>>(
                _subjectService.FindMany(subject => subject.Release.Id == releaseId));
        }
    }
}