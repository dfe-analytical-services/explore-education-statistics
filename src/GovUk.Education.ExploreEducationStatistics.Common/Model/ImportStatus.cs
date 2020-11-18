using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        public bool IsAfterArchiveProcessing()
        {
            return IsAfterStage(IStatus.PROCESSING_ARCHIVE_FILE);
        }

        public bool IsAfterStage1()
        {
            return IsAfterStage(IStatus.STAGE_1);
        }
        
        public bool IsAfterStage2()
        {
            return IsAfterStage(IStatus.STAGE_2);
        }
        
        public bool IsAfterStage3()
        {
            return IsAfterStage(IStatus.STAGE_3);
        }
        
        public bool IsAfterStage4()
        {
            return IsAfterStage(IStatus.STAGE_4);
        }
        
        public bool IsAfterStage(IStatus stage)
        {
            return Status.CompareTo(stage) > 0;
        }

        public override string ToString()
        {
            return $"{Status} {PhasePercentageComplete}%, overall {PercentageComplete}%";
        }
    }
}