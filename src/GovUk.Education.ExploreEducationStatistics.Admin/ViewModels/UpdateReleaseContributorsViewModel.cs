#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UpdateReleaseContributorsViewModel
    {
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
