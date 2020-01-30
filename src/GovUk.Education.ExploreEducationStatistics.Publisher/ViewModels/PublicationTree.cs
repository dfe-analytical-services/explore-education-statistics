using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.ViewModels
{
    public class PublicationTree
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }

        public string LegacyPublicationUrl { get; set; }

        public List<FileInfo> DownloadFiles { get; set; }
    }
}