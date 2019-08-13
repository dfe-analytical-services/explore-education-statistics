using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels
{
    public class PublicationTree

    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Summary { get; set; }
        public string LegacyPublicationUrl { get; set; }
        public List<FileInfo> DataFiles { get; set; }
        
        public List<FileInfo> ChartFiles { get; set; }
        
        public List<FileInfo> AncillaryFiles { get; set; }

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