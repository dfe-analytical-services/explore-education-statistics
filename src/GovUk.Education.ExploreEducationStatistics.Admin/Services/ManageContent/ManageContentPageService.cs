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
                    Release = releaseViewModel,
                    IntroductionSection = new ContentSectionViewModel
                    {
                        Id = new Guid("bcb96e42-a09a-4791-a377-9649b0876c58"),
                        Order = 0,
                        Caption = "Introduction section caption",
                        Heading = "Introduction section heading",
                        Content = new List<IContentBlock>
                        {
                            new HtmlBlock
                            {
                                Id = new Guid("65187a0b-eb17-4481-b234-949dc85f1efa"),
                                Type = "MarkDownBlock",
                                Body = "Read national statistical summaries and definitions, view charts and " +
                                       "tables and download data files across a range of pupil absence subject " +
                                       "areas.\n" +
                                       "You can also view a regional breakdown of statistics and data within the " +
                                       "[local authorities section](#)",
                            }
                        }
                    }
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
                .ThenInclude(s => s.Content);
        }
    }
}