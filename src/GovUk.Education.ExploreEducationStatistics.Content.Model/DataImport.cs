#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class DataImport
    {
        private static readonly Dictionary<DataImportStatus, double> ProcessingRatios =
            new()
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

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<DataImportStatus>))]
        public DataImportStatus Status { get; set; }

        public int StagePercentageComplete { get; set; }

        public Guid SubjectId { get; set; }

        public File File { get; set; } = null!;

        public Guid FileId { get; set; }

        public File MetaFile { get; set; } = null!;

        public Guid MetaFileId { get; set; }

        public File? ZipFile { get; set; }

        public Guid? ZipFileId { get; set; }

        public int? TotalRows { get; set; }

        public int NumBatches { get; set; }

        public int RowsPerBatch { get; set; }

        /// <summary>
        /// Note that this means "importable row count" rather than indicating the actual number of rows
        /// already imported.  This is effectively a count of rows that are not excluded from import.
        /// </summary>
        public int? ImportedRows { get; set; }

        public HashSet<GeographicLevel> GeographicLevels { get; set; } = new();

        public List<DataImportError> Errors { get; set; } = new();

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

        /// <summary>
        /// Determines if a file import consists exclusively of one geographic level.
        /// </summary>
        public bool HasSoleGeographicLevel()
        {
            return GeographicLevels is {Count: 1};
        }

        public bool BatchingRequired()
        {
            return NumBatches > 1;
        }

        public override string ToString()
        {
            return $"{Status} {StagePercentageComplete}%, overall {PercentageComplete()}%";
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum DataImportStatus
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

    public static class DataImportStatusExtensions
    {
        private static readonly List<DataImportStatus> FinishedStatuses = new List<DataImportStatus>
        {
            COMPLETE,
            FAILED,
            NOT_FOUND,
            CANCELLED
        };

        private static readonly List<DataImportStatus> AbortingStatuses = new List<DataImportStatus>
        {
            CANCELLING
        };

        public static DataImportStatus GetFinishingStateOfAbortProcess(this DataImportStatus status)
        {
            return status switch
            {
                CANCELLING => CANCELLED,
                _ => throw new ArgumentOutOfRangeException($"No final abort state exists for state {status}")
            };
        }

        public static bool IsFinished(this DataImportStatus state)
        {
            return FinishedStatuses.Contains(state);
        }

        public static bool IsAborting(this DataImportStatus state)
        {
            return AbortingStatuses.Contains(state);
        }

        public static bool IsFinishedOrAborting(this DataImportStatus state)
        {
            return IsFinished(state) || IsAborting(state);
        }
    }
}
