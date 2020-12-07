using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class PublicationDownloadTreeNode : AbstractPublicationTreeNode
    {
        public List<FileInfo> DownloadFiles { get; set; }

        public string EarliestReleaseTime { get; set; }

        public string LatestReleaseTime { get; set; }
    }
}