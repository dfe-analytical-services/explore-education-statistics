using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics
{
    public class FootnoteTimePeriodViewModel
    {
        public int Year { get; set; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier TimeIdentifier { get; set; }
    }
}
