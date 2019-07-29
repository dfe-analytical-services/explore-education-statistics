using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImporterService _importerService;
        private readonly ILogger<IFileImportService> _logger;

        public FileImportService(
            ApplicationDbContext context,
            IFileStorageService fileStorageService,
            IImporterService importerService,
            ILogger<IFileImportService> logger)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _importerService = importerService;
            _logger = logger;
        }

        public void ImportFiles(ImportMessage message)
        {
            var subjectData = _fileStorageService.GetSubjectData(message).Result;
            
            var subject = _context.Subject
                .Include(s => s.Release)
                .ThenInclude(r => r.Publication)
                .ThenInclude(p => p.Topic)
                .ThenInclude(t => t.Theme)
                .FirstOrDefault(s => s.Name.Equals(subjectData.Name) && s.ReleaseId == message.Release.Id);

            // If this is a new subject then there will be no observations so process the data from
            // the meta file for the first time.

            var subjectMeta = _importerService.ImportMeta(subjectData.GetMetaLines().ToList(), subject, true);
            
            _importerService.ImportObservations(subjectData.GetCsvLines().ToList(), subject, subjectMeta);
            
            _logger.LogInformation("Import of data lines from {0} completed", 
                BlobUtils.GetName(subjectData.DataBlob));
        }
    }
}