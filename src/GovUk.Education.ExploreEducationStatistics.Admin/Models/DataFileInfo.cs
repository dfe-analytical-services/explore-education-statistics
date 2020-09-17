using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class DataFileInfo : FileInfo
    {
        public string MetaFileName { get; set; }
        public int Rows { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset? Created { get; set; }
    }
}