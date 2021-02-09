using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class DataImportError
    {
        public Guid Id { get; set; }

        public Guid DataImportId { get; set; }

        public DataImport DataImport { get; set; }

        public DateTime Created { get; set; }

        public string Message { get; set; }
        
        public DataImportError(string message)
        {
            Created = DateTime.UtcNow;
            Message = message;
        }
    }
}
