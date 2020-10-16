using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class DataFileInfo : FileInfo
    {
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<ReleaseFileTypes>))]
        public new readonly ReleaseFileTypes Type = ReleaseFileTypes.Data;

        public Guid? MetaFileId { get; set; }
        public string MetaFileName { get; set; }
        public int Rows { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset? Created { get; set; }
        public Guid? ReplacedBy { get; set; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<IStatus>))]
        public IStatus Status { get; set; }
    }
}