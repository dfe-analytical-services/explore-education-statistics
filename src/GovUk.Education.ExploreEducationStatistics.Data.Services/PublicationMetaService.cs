using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class PublicationMetaService : IPublicationMetaService
    {
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public PublicationMetaService(
            IReleaseService releaseService,
            ISubjectService subjectService,
            IMapper mapper)
        {
            _releaseService = releaseService;
            _subjectService = subjectService;
            _mapper = mapper;
        }

        public PublicationSubjectsMetaViewModel GetSubjectsForLatestRelease(Guid publicationId)
        {
            var releaseId = _releaseService.GetLatestRelease(publicationId);

            var subjectMetaViewModels = _mapper.Map<IEnumerable<IdLabel>>(
                _subjectService.FindMany(subject => subject.Release.Id == releaseId));

            return new PublicationSubjectsMetaViewModel
            {
                PublicationId = publicationId,
                Subjects = subjectMetaViewModels
            };
        }
    }
}