using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class DataGuidanceViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public List<DataGuidanceSubjectViewModel> Subjects { get; set; }
    }

    public class DataGuidanceUpdateViewModel
    {
        public string Content { get; set; }
        public List<DataGuidanceUpdateSubjectViewModel> Subjects { get; set; }
    }

    public class DataGuidanceUpdateSubjectViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
    }
}