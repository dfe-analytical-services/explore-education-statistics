using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class FileInfo
    {
        public string Extension { get; set; }
        public string Name { get; set; }

        public string FileName => Path?.Substring(Path.LastIndexOf('/') + 1);
        
        public string Path { get; set; }
        public string Size { get; set; }
        public string MetaFileName { get; set; }
        public int Rows { get; set; }
        public string UserName { get; set; }

        public DateTimeOffset? Created { get; set; }
    }
}