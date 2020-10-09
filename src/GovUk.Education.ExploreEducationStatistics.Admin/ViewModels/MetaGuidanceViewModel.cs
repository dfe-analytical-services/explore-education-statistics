using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class MetaGuidanceViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public List<MetaGuidanceSubjectViewModel> Subjects { get; set; }
    }

    public class MetaGuidanceSubjectViewModel
    {
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public MetaGuidanceSubjectTimePeriodsViewModel TimePeriods { get; set; }
        [JsonProperty (ItemConverterType = typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
        public List<GeographicLevel> GeographicLevels { get; set; }
        public List<LabelValue> Variables { get; set; }
    }

    public class MetaGuidanceSubjectTimePeriodsViewModel
    {
        public string From { get;  }
        public string To { get; }

        public MetaGuidanceSubjectTimePeriodsViewModel(string from, string to)
        {
            From = from;
            To = to;
        }
    }
    
    public class MetaGuidanceUpdateReleaseViewModel
    {
        public string Content { get; set; }
    }
    
    public class MetaGuidanceUpdateSubjectViewModel
    {
        public string Content { get; set; }
    }
}