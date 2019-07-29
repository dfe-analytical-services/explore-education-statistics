using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;

        public FileImportService(
            ApplicationDbContext context,
            IFileStorageService fileStorageService,
            IImporterService importerService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _importerService = importerService;
        }

        public void ImportFiles(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var batch = subjectData.GetCsvLines().ToList();
            var metaLines = subjectData.GetMetaLines().ToList();
            
            var subject = _context.Subject
                .Include(s => s.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(s => s.Name.Equals(subjectData.Name) && s.ReleaseId == message.Release.Id);

            _importerService.ImportObservations(
                batch,
                subject,
                _importerService.GetMeta(metaLines, subject));
        }
        
        public void ImportFilters(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            var batch = subjectData.GetCsvLines().ToList();
            var metaLines = subjectData.GetMetaLines().ToList();
            
            var subject = _context.Subject
                .Include(s => s.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(s => s.Name.Equals(subjectData.Name) && s.ReleaseId == message.Release.Id);
            
            _importerService.ImportFiltersAndLocations(
                batch,
                _importerService.GetMeta(metaLines, subject));
        }
    }
}