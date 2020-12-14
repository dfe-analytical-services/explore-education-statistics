using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.IStatus;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class ImportStatus
    {
        private static readonly List<IStatus> FinishedStatuses = new List<IStatus>
        {
            COMPLETE,
            FAILED,
            NOT_FOUND,
            CANCELLED
        };

        private static readonly List<IStatus> AbortingStatuses = new List<IStatus>
        {
            CANCELLING,
        };
        
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<IStatus>))]
        public IStatus Status { get; set; }

        public int PercentageComplete { get; set; }

        public int PhasePercentageComplete { get; set; }

        public string Errors { get; set; }

        public int NumberOfRows { get; set; }
        
        public bool IsFinished()
        {
            return IsFinishedState(Status);
        }

        public bool IsFinishedOrAborting()
        {
            return IsFinishedOrAbortingState(Status);
        }
        
        public static bool IsFinishedState(IStatus state)
        {
            return FinishedStatuses.Contains(state);
        }
        
        public static bool IsFinishedOrAbortingState(IStatus state)
        {
            return IsFinishedState(state) || AbortingStatuses.Contains(state);
        }

        public override string ToString()
        {
            return $"{Status} {PhasePercentageComplete}%, overall {PercentageComplete}%";
        }
    }
}