using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ContentService : IContentService
    {
        private readonly ApplicationDbContext _context;
        private readonly PersistenceHelper<Release, Guid> _releaseHelper; 

        public ContentService(ApplicationDbContext context)
        {
            _context = context;
            _releaseHelper = new PersistenceHelper<Release, Guid>(
                _context, 
                context.Releases, 
                ValidationErrorMessages.ReleaseNotFound);
        }

        public Task<Either<ValidationResult, List<ContentSectionViewModel>>> GetContentSectionsAsync(
            Guid releaseId)
        {
            return _releaseHelper.CheckEntityExists(releaseId, release => 
                release
                    .Content
                    .Select(ContentSectionViewModel.ToViewModel)
                    .OrderBy(c => c.Order)
                    .ToList(),
                HydrateContentSectionsAndBlocks);
        }

        public Task<Either<ValidationResult, List<ContentSectionViewModel>>> ReorderContentSectionsAsync(
            Guid releaseId, 
            Dictionary<Guid, int> newSectionOrder)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
            {
                newSectionOrder.ToList().ForEach(kvp =>
                {
                    var (sectionId, newOrder) = kvp;
                    release.Content.Find(section => section.Id == sectionId).Order = newOrder;
                });
                
                _context.Releases.Update(release);
                await _context.SaveChangesAsync();
                return OrderedContentSections(release);
            }, HydrateContentSectionsAndBlocks);
        }

        public Task<Either<ValidationResult, ContentSectionViewModel>> AddContentSectionAsync(
            Guid releaseId)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
            {
                var maxOrderNumber = release.Content.Max(section => section.Order);
                release.Content.Add(new ContentSection
                {
                    Heading = "New section",
                    Order = maxOrderNumber + 1
                });
                
                _context.Releases.Update(release);
                await _context.SaveChangesAsync();
                return OrderedContentSections(release).Last();
            }, HydrateContentSectionsAndBlocks);
        }
        
        public Task<Either<ValidationResult, List<ContentSectionViewModel>>> RemoveContentSectionAsync(
            Guid releaseId,
            Guid contentSectionId)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
            {
                var sectionToRemove = release
                    .Content
                    .Find(contentSection => contentSection.Id == contentSectionId);

                if (sectionToRemove == null)
                {
                    return new Either<ValidationResult, List<ContentSectionViewModel>>(
                        ValidationUtils.ValidationResult(ValidationErrorMessages.ContentSectionNotFound));
                }

                release.Content.Remove(sectionToRemove);

                var removedSectionOrder = sectionToRemove.Order;
                
                release.Content.ForEach(contentSection =>
                {
                    if (contentSection.Order > removedSectionOrder)
                    {
                        contentSection.Order--;
                    }
                });
                
                _context.Releases.Update(release);
                await _context.SaveChangesAsync();
                return OrderedContentSections(release);
            }, HydrateContentSectionsAndBlocks);
        }

        private static List<ContentSectionViewModel> OrderedContentSections(Release release)
        {
            return release
                .Content
                .Select(ContentSectionViewModel.ToViewModel)
                .OrderBy(c => c.Order)
                .ToList();
        }

        private static IQueryable<Release> HydrateContentSectionsAndBlocks(IQueryable<Release> releases)
        {
            return releases
                .Include(r => r.Content)
                .ThenInclude(section => section.Content);
        }
    }
}