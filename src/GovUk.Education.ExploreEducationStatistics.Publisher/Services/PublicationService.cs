using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;

        public PublicationService(
            ContentDbContext contentDbContext,
            IMapper mapper,
            IReleaseService releaseService)
        {
            _contentDbContext = contentDbContext;
            _mapper = mapper;
            _releaseService = releaseService;
        }

        public async Task<Publication> Get(Guid id)
        {
            return await _contentDbContext.Publications.FindAsync(id);
        }

        public async Task<CachedPublicationViewModel> GetViewModel(Guid id, IEnumerable<Guid> includedReleaseIds)
        {
            var publication = await _contentDbContext.Publications
                .Include(p => p.Contact)
                .Include(p => p.LegacyReleases)
                .Include(p => p.Topic)
                .ThenInclude(topic => topic.Theme)
                .SingleOrDefaultAsync(p => p.Id == id);

            var publicationViewModel = _mapper.Map<CachedPublicationViewModel>(publication);
            var latestRelease = await _releaseService.GetLatestRelease(publication.Id, includedReleaseIds);
            publicationViewModel.LatestReleaseId = latestRelease.Id;
            publicationViewModel.Releases = GetReleaseViewModels(id, includedReleaseIds);
            return publicationViewModel;
        }

        public List<Publication> GetPublicationsWithPublishedReleases()
        {
            return _contentDbContext.Publications
                .Include(publication => publication.Releases)
                .ToList()
                .Where(p => p.Releases.Any(release => release.IsLatestPublishedVersionOfRelease()))
                .ToList();
        }

        public async Task<bool> IsPublicationPublished(Guid publicationId)
        {
            var publication = await _contentDbContext.Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == publicationId);

            return publication.Releases.Any(release => release.IsLatestPublishedVersionOfRelease());
        }

        public async Task SetPublishedDate(Guid id, DateTime published)
        {
            var publication = await Get(id);

            if (publication == null)
            {
                throw new ArgumentException("Publication does not exist", nameof(id));
            }

            publication.Published = published;

            await UpdatePublication(publication);
        }

        private async Task UpdatePublication(Publication publication)
        {
            _contentDbContext.Update(publication);
            await _contentDbContext.SaveChangesAsync();
        }

        private List<ReleaseTitleViewModel> GetReleaseViewModels(
            Guid publicationId,
            IEnumerable<Guid> includedReleaseIds)
        {
            var releases = _contentDbContext.Releases
                .Include(r => r.Publication)
                .Where(release => release.PublicationId == publicationId)
                .ToList()
                .Where(release => release.IsReleasePublished(includedReleaseIds))
                .OrderByDescending(release => release.Year)
                .ThenByDescending(release => release.TimePeriodCoverage);
            return _mapper.Map<List<ReleaseTitleViewModel>>(releases);
        }
    }
}
