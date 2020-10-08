using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class MetaGuidanceViewModel
    {
        public string Content { get; set; }
        public List<MetaGuidanceSubjectViewModel> Subjects { get; set; }
    }

    public class MetaGuidanceSubjectViewModel
    {
        public string Filename { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public List<GeographicLevel> GeographicLevels { get; set; }
    }

    public class MetaGuidanceUpdateViewModel
    {
        public string Content { get; set; }
    }
}