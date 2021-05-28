using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class AdminFileInfo : FileInfo
    {
        public string UserName { get; set; }
        public DateTime? Created { get; set; }
    }
}
