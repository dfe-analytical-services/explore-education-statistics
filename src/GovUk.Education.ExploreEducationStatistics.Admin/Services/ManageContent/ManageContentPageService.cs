using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using static GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils.ReleaseUtils;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ManageContentPageService : IManageContentPageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ManageContentPageService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        
        public Task<Either<ValidationResult, ManageContentPageViewModel>> GetManageContentPageViewModelAsync(Guid releaseId)
        {
            return CheckReleaseExists(_context, releaseId, release => 
                new ManageContentPageViewModel()
                {
                    Release = _mapper.Map<ReleaseViewModel>(release)
                }
            , HydrateReleaseForReleaseViewModel);
        }
        
        private static IQueryable<Release> HydrateReleaseForReleaseViewModel(IQueryable<Release> values)
        {
            return values
                .Include(r => r.Publication)
                .ThenInclude(publication => publication.Contact)
                .Include(r => r.Type);
        }
    }
}