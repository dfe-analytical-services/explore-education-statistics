#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public record DataFileInfo : FileInfo
    {
        public override FileType Type { get; set; } = FileType.Data;

        public Guid? MetaFileId { get; set; }

        public string MetaFileName { get; set; } = string.Empty;

        public int? Rows { get; set; }

        public Guid? ReplacedBy { get; set; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<DataImportStatus>))]
        public DataImportStatus Status { get; set; }

        public DataFilePermissions Permissions { get; set; } = new();

        public bool IsLinkedToApiDataSet { get; set; }
    }
}
