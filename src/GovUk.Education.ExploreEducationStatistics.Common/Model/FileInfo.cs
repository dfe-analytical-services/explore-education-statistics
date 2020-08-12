using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class FileInfo
    {
        public Guid Id { get; set; }
        public string Extension { get; set; }
        public string Name { get; set; }
        public string FileName => Path?.Substring(Path.LastIndexOf('/') + 1);
        public string Path { get; set; }
        public string Size { get; set; }
    }
}