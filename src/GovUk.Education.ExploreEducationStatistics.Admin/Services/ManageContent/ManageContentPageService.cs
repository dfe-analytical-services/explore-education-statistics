using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ManageContentPageService : IManageContentPageService
    {
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly PersistenceHelper<Release, Guid> _releaseHelper;

        public ManageContentPageService(ContentDbContext context, IMapper mapper,
            IFileStorageService fileStorageService)
        {
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _releaseHelper = new PersistenceHelper<Release, Guid>(
                context,
                context.Releases,
                ValidationErrorMessages.ReleaseNotFound);
        }

        public Task<Either<ValidationResult, ManageContentPageViewModel>> GetManageContentPageViewModelAsync(
            Guid releaseId)
        {
            return _releaseHelper.CheckEntityExists(releaseId, release =>
            {
                var releaseViewModel = _mapper.Map<ReleaseViewModel>(release);
                releaseViewModel.DownloadFiles = _fileStorageService.ListPublicFilesPreview(releaseId);
                return new ManageContentPageViewModel
                {
                    Release = releaseViewModel
                };
            }, HydrateReleaseForReleaseViewModel);
        }
        
        private static IQueryable<Release> HydrateReleaseForReleaseViewModel(IQueryable<Release> values)
        {
            return values
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Contact)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Releases)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.LegacyReleases)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Methodology)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Topic.Theme)
                .Include(r => r.Type)
                .Include(r => r.Content)
                .ThenInclude(join => join.ContentSection)
                .ThenInclude(section => section.Content);
        }
    }
}