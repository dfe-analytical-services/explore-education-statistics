using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class ImportStatus
    {
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<IStatus>))]
        public IStatus Status { get; set; }

        public int PercentageComplete { get; set; }

        public int PhasePercentageComplete { get; set; }

        public string Errors { get; set; }

        public int NumberOfRows { get; set; }

        public bool PhaseComplete => PhasePercentageComplete == 100;

        public override string ToString()
        {
            return $"{Status} {PhasePercentageComplete}%, overall {PercentageComplete}%";
        }
    }
}