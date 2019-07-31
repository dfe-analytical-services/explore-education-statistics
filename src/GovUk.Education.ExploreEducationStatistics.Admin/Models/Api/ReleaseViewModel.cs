using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class ReleaseViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string ReleaseName { get; set; }
        
        public string CoverageTitle { get; set; }
        
        public string YearTitle { get; set; }
        
        public DateTime? Published { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        public bool LatestRelease { get; set; }
    }
}