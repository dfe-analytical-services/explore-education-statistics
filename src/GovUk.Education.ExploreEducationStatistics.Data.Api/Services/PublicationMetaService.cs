using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
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

        public PublicationSubjectsMetaViewModel GetPublication(Guid publicationId)
        {
            var releaseId = _releaseService.GetLatestRelease(publicationId);

            var subjectMetaViewModels = _mapper.Map<IEnumerable<IdLabelViewModel>>(
                _subjectService.FindMany(subject => subject.Release.Id == releaseId));

            return new PublicationSubjectsMetaViewModel
            {
                PublicationId = publicationId,
                Subjects = subjectMetaViewModels
            };
        }

        public IEnumerable<ThemeMetaViewModel> GetThemes()
        {
            return _mapper.Map<IEnumerable<ThemeMetaViewModel>>(_releaseService.All(
                    new List<Expression<Func<Release, object>>>
                    {
                        release => release.Publication.Topic.Theme
                    })
                .GroupBy(release => release.Publication.Topic.Theme)
                .Select(grouping => grouping.Key)
            );
        }
    }
}