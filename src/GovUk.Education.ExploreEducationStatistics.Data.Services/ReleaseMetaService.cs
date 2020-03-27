using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<IdLabel>> GetSubjectsAsync(Guid releaseId)
        {
            var subjects = await _subjectService.GetSubjectsForReleaseAsync(releaseId);
            return _mapper.Map<IEnumerable<IdLabel>>(subjects);
        }
    }
}