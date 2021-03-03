using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class RelatedInformationService : IRelatedInformationService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper; 
        private readonly IUserService _userService; 

        public RelatedInformationService(ContentDbContext context, IPersistenceHelper<ContentDbContext> persistenceHelper, 
            IUserService userService)
        {
            _context = context;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
        }
        
        public Task<Either<ActionResult, List<Link>>> GetRelatedInformationAsync(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(release => release.RelatedInformation);
        }

        public Task<Either<ActionResult, List<Link>>> AddRelatedInformationAsync(Guid releaseId, CreateUpdateLinkRequest request)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    if (release.RelatedInformation == null)
                    {
                        release.RelatedInformation = new List<Link>();
                    }
                    
                    release.RelatedInformation.Add(new Link
                    {
                        Id = Guid.NewGuid(),
                        Description = request.Description,
                        Url = request.Url
                    });

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return release.RelatedInformation;
                });
        }
        
        public Task<Either<ActionResult, List<Link>>> UpdateRelatedInformationAsync(
            Guid releaseId, Guid relatedInformationId, CreateUpdateLinkRequest request)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var toUpdate = release
                        .RelatedInformation
                        .Find(item => item.Id == relatedInformationId);

                    if (toUpdate == null)
                    {
                        return NotFound<List<Link>>();
                    }

                    toUpdate.Description = request.Description;
                    toUpdate.Url = request.Url;

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return release.RelatedInformation;
                });
        }
        
        public Task<Either<ActionResult, List<Link>>> DeleteRelatedInformationAsync(Guid releaseId, Guid relatedInformationId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    release.RelatedInformation.Remove(
                        release.RelatedInformation.Find(item => item.Id == relatedInformationId));

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return release.RelatedInformation;
                });
        }
    }
}
