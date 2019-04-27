namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
    using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
    using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
    using Microsoft.Extensions.Logging;

    public class SeedService : ISeedService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IImporterService _importerService;

        public SeedService(ILogger<SeedService> logger,
            ApplicationDbContext context,
            IImporterService importerService)
        {
            _logger = logger;
            _context = context;
            _importerService = importerService;
        }
        
        public void Seed(Publication publication)
        {
            _logger.LogInformation("Seeding Publication {Publication}", publication.PublicationId);

            foreach (var release in publication.Releases)
            {
                SeedRelease(release);
            }
        }

        private void SeedRelease(Release release)
        {
            _logger.LogInformation("Seeding Release for {Publication}, {Release}", release.PublicationId,
                release.Name);

            var releaseDb = CreateRelease(release);

            foreach (var subject in release.Subjects)
            {
                SeedSubject(releaseDb, subject);
            }
        }

        private void SeedSubject(Model.Release release, Subject subject)
        {
            _logger.LogInformation("Seeding Subject for {Publication}, {Subject}", release.PublicationId,
                subject.Name);

            var subjectDb = CreateSubject(release, subject);

            var lines = subject.GetCsvLines();
            var metaLines = subject.GetMetaLines();
            
            _importerService.Import(lines, metaLines, subjectDb);
        }

        private Model.Release CreateRelease(Release release)
        {
            var releaseDb = _context.Release.Add(new Model.Release(release.ReleaseDate, release.PublicationId)).Entity;
            _context.SaveChanges();
            return releaseDb;
        }

        private Model.Subject CreateSubject(Model.Release release, Subject subject)
        {
            var subjectDb = _context.Subject.Add(new Model.Subject(subject.Name, release)).Entity;
            _context.SaveChanges();
            return subjectDb;
        }
    }
}