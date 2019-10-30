using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

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

        public ReleaseSubjectsMetaViewModel GetSubjects(Guid releaseId)
        {
            var subjectMetaViewModels = _mapper.Map<IEnumerable<IdLabel>>(
                _subjectService.FindMany(subject => subject.Release.Id == releaseId));

            return new ReleaseSubjectsMetaViewModel
            {
                ReleaseId = releaseId,
                Subjects = subjectMetaViewModels
            };
        }
    }
}