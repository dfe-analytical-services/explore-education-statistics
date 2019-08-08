using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger _logger;


        public DownloadService(ApplicationDbContext context,
            IFileStorageService fileStorageService,
            ILogger<DownloadService> logger)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public IEnumerable<ThemeTree> GetDownloadTree()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Publication, PublicationTree>()
                    .ForMember(
                        dest => dest.DataFiles, m => m.MapFrom(publication =>
                            ListFiles(publication.Slug, GetLatestRelease(publication).Slug, ReleaseFileTypes.Data))); // TODO qqRP
            });

            var mapper = config.CreateMapper();

            var themes = GetReleases()
                .GroupBy(release => release.Publication.Topic.Theme)
                .Select(grouping => grouping.Key);

            return mapper.Map<IEnumerable<ThemeTree>>(themes);
        }

        private static Release GetLatestRelease(Publication publication)
        {
            return publication.Releases.ToList()
                .OrderByDescending(release => release.Published)
                .FirstOrDefault();
        }

        private IEnumerable<Release> GetReleases()
        {
            return _context.Releases.Include(release => release.Publication.Topic.Theme);
        }

        private IEnumerable<FileInfo> ListFiles(string publication, string release, ReleaseFileTypes type)
        {
            return _fileStorageService.ListFiles(publication, release, type);
        }
    }
}