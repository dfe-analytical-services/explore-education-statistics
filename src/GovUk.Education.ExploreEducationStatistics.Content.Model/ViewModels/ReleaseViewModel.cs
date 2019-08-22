using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels
{
    public class ReleaseViewModel
    {
        public Guid Id { get; set; }

        [Required] public string Title { get; set; }

        public string ReleaseName { get; set; }
        
        public string CoverageTitle { get; set; }
        
        public string YearTitle { get; set; }

        public DateTime? Published { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }

        public List<Update> Updates { get; set; }

        public List<ContentSection> Content { get; set; }

        public DataBlock KeyStatistics { get; set; }
        
        public List<FileInfo> DataFiles { get; set; }
        
        public List<FileInfo> AncillaryFiles { get; set; }
        
        public List<FileInfo> ChartFiles { get; set; }
        
        
        // Files to download are the actual data files and ancillary files, but currently not the chart files.
        public List<FileInfo> DownloadFiles
        {
            get
            {
                var data = DataFiles ?? new List<FileInfo>();
                var ancillary = AncillaryFiles ?? new List<FileInfo>();
                return data.Concat(ancillary).ToList();
            }
        }
    }
}