using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImportService : IImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;

        public ImportService(
            ApplicationDbContext context,
            IFileStorageService fileStorageService,
            IImporterService importerService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _importerService = importerService;
        }

        public void Import(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;

            // TODO DFE-866 Create or update the Theme/Topic/Publication/Release.
            // TODO DFE-866 Currently we assume that it already exists. No update happens.

            var release = _context.Release
                .Where(r => r.Id.Equals(message.Release.Id))
                .Include(r => r.Publication)
                .FirstOrDefault();

            var subject = CreateSubject(subjectData.Name, release);
            _context.SaveChanges();

            _importerService.Import(subjectData.GetCsvLines(), subjectData.GetMetaLines(), subject);
        }

        private Subject CreateSubject(string name, Release release)
        {
            var subject = _context.Subject.Add(new Subject(name, release)).Entity;
            return subject;
        }
    }
}