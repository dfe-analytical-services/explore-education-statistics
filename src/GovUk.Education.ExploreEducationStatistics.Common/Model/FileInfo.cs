#nullable enable
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public record FileInfo
{
    public Guid? Id { get; set; }

    public string Extension => Path.GetExtension(FileName).TrimStart('.');

    public required string FileName { get; set; }

    public required string Name { get; set; }

    public string? Summary { get; set; }

    public required string Size { get; set; }

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<FileType>))]
    [Newtonsoft.Json.JsonConverter(typeof(Converters.EnumToEnumValueJsonConverter<FileType>))]
    public virtual FileType Type { get; set; }

    public DateTime? Created { get; set; }

    public string? UserName { get; set; }
}
