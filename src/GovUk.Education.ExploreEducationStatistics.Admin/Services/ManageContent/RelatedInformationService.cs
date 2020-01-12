using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class RelatedInformationService : IRelatedInformationService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<Release, Guid> _releaseHelper; 
        private readonly IUserService _userService; 

        public RelatedInformationService(ContentDbContext context, IPersistenceHelper<Release, Guid> releaseHelper, 
            IUserService userService)
        {
            _context = context;
            _releaseHelper = releaseHelper;
            _userService = userService;
        }
        
        public Task<Either<ActionResult, List<BasicLink>>> GetRelatedInformationAsync(Guid releaseId)
        {
            return _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(release => release.RelatedInformation);
        }

        public Task<Either<ActionResult, List<BasicLink>>> AddRelatedInformationAsync(Guid releaseId, CreateUpdateLinkRequest request)
        {
            return _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    if (release.RelatedInformation == null)
                    {
                        release.RelatedInformation = new List<BasicLink>();
                    }
                    
                    release.RelatedInformation.Add(new BasicLink
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
        
        public Task<Either<ActionResult, List<BasicLink>>> UpdateRelatedInformationAsync(
            Guid releaseId, Guid relatedInformationId, CreateUpdateLinkRequest request)
        {
            return _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var toUpdate = release
                        .RelatedInformation
                        .Find(item => item.Id == relatedInformationId);

                    if (toUpdate == null)
                    {
                        return NotFound<List<BasicLink>>();
                    }

                    toUpdate.Description = request.Description;
                    toUpdate.Url = request.Url;

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return release.RelatedInformation;
                });
        }
        
        public Task<Either<ActionResult, List<BasicLink>>> DeleteRelatedInformationAsync(Guid releaseId, Guid relatedInformationId)
        {
            return _releaseHelper
                .CheckEntityExistsActionResult(releaseId)
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