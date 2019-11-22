using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
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
        private readonly PersistenceHelper<Release, Guid> _releaseHelper; 

        public ManageContentPageService(ContentDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _releaseHelper = new PersistenceHelper<Release, Guid>(
                context, 
                context.Releases, 
                ValidationErrorMessages.ReleaseNotFound);
        }
        
        public Task<Either<ValidationResult, ManageContentPageViewModel>> GetManageContentPageViewModelAsync(Guid releaseId)
        {
            return _releaseHelper.CheckEntityExists(releaseId, release =>
                new ManageContentPageViewModel
                {
                    Release = _mapper.Map<ReleaseViewModel>(release),
                    RelatedInformation = new List<BasicLink>()
                    {
                        new BasicLink()
                        {
                            Id = new Guid("15a8dbb8-d8b7-4247-b841-e798860a4700"),
                            Description = "Pupil absence statistics: guidance and methodology",
                            Url = "http://example.com/1"
                        },
                        new BasicLink()
                        {
                            Id = new Guid("fadedcb1-f386-4faa-a24f-6731be534097"),
                            Description = "This is an example of a related information link",
                            Url = "http://example.com/2"
                        }
                    },
                    IntroductionSection = new ContentSectionViewModel()
                    {
                        Id = new Guid("bcb96e42-a09a-4791-a377-9649b0876c58"),
                        Order = 0,
                        Caption = "Introduction section caption",
                        Heading = "Introduction section heading",
                        Content = new List<IContentBlock>()
                        {
                            new HtmlBlock()
                            {
                                Id = new Guid("65187a0b-eb17-4481-b234-949dc85f1efa"),
                                Type = "HtmlBlock",
                                Body = "<p>Read national statistical summaries and definitions, view charts and " +
                                       "tables and download data files across a range of pupil absence subject " +
                                       "areas.</p>" + 
                                       "<p>You can also view a regional breakdown of statistics and data within the " +
                                       "<a href=\"#\">local authorities section</a></p>",
                            }
                        }
                    },
                }
            , HydrateReleaseForReleaseViewModel);
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
                // TODO
                .ThenInclude(publication => publication.Methodology)
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Topic.Theme)
                .Include(r => r.Type);
        }
    }
}