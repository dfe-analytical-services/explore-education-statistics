using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class MetaGuidanceService : IMetaGuidanceService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IMetaGuidanceSubjectService _metaGuidanceSubjectService;

        public MetaGuidanceService(
            IFileStorageService fileStorageService,
            IMetaGuidanceSubjectService metaGuidanceSubjectService)
        {
            _fileStorageService = fileStorageService;
            _metaGuidanceSubjectService = metaGuidanceSubjectService;
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Get(string releasePath)
        {
            return await _fileStorageService.GetDeserialized<CachedReleaseViewModel>(releasePath)
                .OnSuccess(
                    async release => await _metaGuidanceSubjectService.GetSubjects(release.Id)
                        .OnSuccess(
                            subjects =>
                                new MetaGuidanceViewModel
                                {
                                    Id = release.Id,
                                    Content = release.MetaGuidance,
                                    Subjects = subjects
                                }
                        )
                );
        }
    }
}