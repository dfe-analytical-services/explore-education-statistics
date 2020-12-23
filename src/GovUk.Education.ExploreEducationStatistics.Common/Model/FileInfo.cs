using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class FileInfo
    {
        public Guid? Id { get; set; }
        public string Extension => FileStorageUtils.GetExtension(FileName);
        public string FileName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Size { get; set; }

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<ReleaseFileTypes>))]
        public ReleaseFileTypes Type { get; set; }
    }
}