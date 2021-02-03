using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ImportError
    {
        public Guid Id { get; set; }

        public Guid ImportId { get; set; }

        public Import Import { get; set; }

        public DateTime Created { get; set; }

        public string Message { get; set; }
        
        public ImportError(string message)
        {
            Created = DateTime.UtcNow;
            Message = message;
        }
    }
}