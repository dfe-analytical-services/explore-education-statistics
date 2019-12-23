using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class RelatedInformationService : IRelatedInformationService
    {
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<Release, Guid> _releaseHelper; 

        public RelatedInformationService(ContentDbContext context, IPersistenceHelper<Release, Guid> releaseHelper)
        {
            _context = context;
            _releaseHelper = releaseHelper;
        }
        
        public Task<Either<ValidationResult, List<BasicLink>>> GetRelatedInformationAsync(Guid releaseId)
        {
            return _releaseHelper
                .CheckEntityExists(releaseId)
                .OnSuccess(release => release.RelatedInformation);
        }

        public Task<Either<ValidationResult, List<BasicLink>>> AddRelatedInformationAsync(Guid releaseId, CreateUpdateLinkRequest request)
        {
            return _releaseHelper
                .CheckEntityExists(releaseId)
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
        
        public Task<Either<ValidationResult, List<BasicLink>>> UpdateRelatedInformationAsync(
            Guid releaseId, Guid relatedInformationId, CreateUpdateLinkRequest request)
        {
            return _releaseHelper
                .CheckEntityExists(releaseId)
                .OnSuccess(async release =>
                {
                    var toUpdate = release.RelatedInformation.Find(item => item.Id == relatedInformationId);

                    if (toUpdate == null)
                    {
                        return ValidationResult(ValidationErrorMessages.RelatedInformationItemNotFound);
                    }

                    toUpdate.Description = request.Description;
                    toUpdate.Url = request.Url;

                    _context.Releases.Update(release);
                    await _context.SaveChangesAsync();
                    return new Either<ValidationResult, List<BasicLink>>(release.RelatedInformation);
                });
        }
        
        public Task<Either<ValidationResult, List<BasicLink>>> DeleteRelatedInformationAsync(Guid releaseId, Guid relatedInformationId)
        {
            return _releaseHelper
                .CheckEntityExists(releaseId)
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