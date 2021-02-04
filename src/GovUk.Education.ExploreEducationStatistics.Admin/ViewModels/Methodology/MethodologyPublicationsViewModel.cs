using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyPublicationsViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }

        public List<IdTitlePair> Publications { get; set; }
    }
}