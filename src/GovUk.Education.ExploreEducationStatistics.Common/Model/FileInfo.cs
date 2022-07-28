#nullable enable
using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class FileInfo
    {
        public Guid? Id { get; set; }

        public string Extension => Path.GetExtension(FileName).TrimStart('.');

        public string FileName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public string Size { get; set; } = string.Empty;

        [JsonConverter(typeof(EnumToEnumValueJsonConverter<FileType>))]
        public FileType Type { get; set; }

        public DateTime? Created { get; set; }

        public string? UserName { get; set; }
    }
}
