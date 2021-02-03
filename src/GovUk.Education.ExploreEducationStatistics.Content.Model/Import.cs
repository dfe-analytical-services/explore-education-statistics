using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Import
    {
        private static readonly Dictionary<ImportStatus, double> ProcessingRatios = new Dictionary<ImportStatus, double>
        {
            {STAGE_1, .1},
            {STAGE_2, .1},
            {STAGE_3, .1},
            {STAGE_4, .7},
            {CANCELLING, 1},
            {CANCELLED, 1},
            {COMPLETE, 1},
        };

        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<ImportStatus>))]
        public ImportStatus Status { get; set; }

        public int StagePercentageComplete { get; set; }

        public Guid SubjectId { get; set; }
        
        public File File { get; set; }

        public Guid FileId { get; set; }

        public File MetaFile { get; set; }

        public Guid MetaFileId { get; set; }

        public File ZipFile { get; set; }

        public Guid? ZipFileId { get; set; }

        public int Rows { get; set; }

        public int NumBatches { get; set; }

        public int RowsPerBatch { get; set; }

        public int TotalRows { get; set; }

        // EES-1231 Temporary column to differentiate rows migrated from table storage from newer imports 
        public bool Migrated { get; set; }

        public List<ImportError> Errors { get; set; }

        public int PercentageComplete()
        {
            return (int) (Status switch
            {
                STAGE_1 => StagePercentageComplete * ProcessingRatios[STAGE_1],
                STAGE_2 => ProcessingRatios[STAGE_1] * 100 +
                           StagePercentageComplete * ProcessingRatios[STAGE_2],
                STAGE_3 => ProcessingRatios[STAGE_1] * 100 +
                           ProcessingRatios[STAGE_2] * 100 +
                           StagePercentageComplete * ProcessingRatios[STAGE_3],
                STAGE_4 => ProcessingRatios[STAGE_1] * 100 +
                           ProcessingRatios[STAGE_2] * 100 +
                           ProcessingRatios[STAGE_3] * 100 +
                           StagePercentageComplete * ProcessingRatios[STAGE_4],
                CANCELLED => ProcessingRatios[CANCELLED] * 100,
                COMPLETE => ProcessingRatios[COMPLETE] * 100,
                _ => 0
            });
        }

        public override string ToString()
        {
            return $"{Status} {StagePercentageComplete}%, overall {PercentageComplete()}%";
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum ImportStatus
    {
        QUEUED,
        PROCESSING_ARCHIVE_FILE,
        STAGE_1, // Basic row validation
        STAGE_2, // Create locations and filters
        STAGE_3, // Split Files
        STAGE_4, // Import observations
        COMPLETE,
        FAILED,
        NOT_FOUND,
        CANCELLING,
        CANCELLED
    }

    public static class ImportStatusExtensions
    {
        private static readonly List<ImportStatus> FinishedStatuses = new List<ImportStatus>
        {
            COMPLETE,
            FAILED,
            NOT_FOUND,
            CANCELLED
        };

        private static readonly List<ImportStatus> AbortingStatuses = new List<ImportStatus>
        {
            CANCELLING
        };

        public static ImportStatus GetFinishingStateOfAbortProcess(this ImportStatus status)
        {
            return status switch
            {
                CANCELLING => CANCELLED,
                _ => throw new ArgumentOutOfRangeException($"No final abort state exists for state {status}")
            };
        }

        public static bool IsFinished(this ImportStatus state)
        {
            return FinishedStatuses.Contains(state);
        }

        public static bool IsAborting(this ImportStatus state)
        {
            return AbortingStatuses.Contains(state);
        }

        public static bool IsFinishedOrAborting(this ImportStatus state)
        {
            return IsFinished(state) || IsAborting(state);
        }
    }
}
