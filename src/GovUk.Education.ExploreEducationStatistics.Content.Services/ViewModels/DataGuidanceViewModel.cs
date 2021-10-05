﻿#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record DataGuidanceViewModel : ReleaseSummaryViewModel
    {
        public string DataGuidance { get;  }

        public List<DataGuidanceSubjectViewModel> Subjects { get; }

        public DataGuidanceViewModel(
            CachedReleaseViewModel release,
            CachedPublicationViewModel publication,
            List<DataGuidanceSubjectViewModel> subjects) : base(release, publication)
        {
            DataGuidance = release.DataGuidance;
            Subjects = subjects;
        }
    }
}
