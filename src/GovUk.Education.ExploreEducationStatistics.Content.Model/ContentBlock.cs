#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

[JsonConverter(typeof(JsonKnownTypesConverter<ContentBlock>))]
[JsonDiscriminator(Name = "Type")]
[KnownType(typeof(DataBlock))]
[KnownType(typeof(HtmlBlock))]
[KnownType(typeof(EmbedBlockLink))]
public abstract class ContentBlock : ICreatedUpdatedTimestamps<DateTime?, DateTime?>
{
    /// <summary>
    /// The maximum time a block can be locked (in minutes).
    /// </summary>
    public const int MaxLockTime = 10;

    public Guid Id { get; set; }

    [JsonIgnore]
    public ContentSection? ContentSection { get; set; }

    [JsonIgnore]
    public Guid? ContentSectionId { get; set; }

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; }

    public int Order { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }

    public List<Comment> Comments { get; set; } = new();

    public DateTime? Locked { get; set; }

    [JsonIgnore]
    public DateTime? LockedUntil => Locked?.AddMinutes(MaxLockTime);

    public User? LockedBy { get; set; }

    [ConcurrencyCheck]
    public Guid? LockedById { get; set; }
}

public class HtmlBlock : ContentBlock
{
    public HtmlBlock() { }

    public string Body { get; set; }
}

public class EmbedBlockLink : ContentBlock
{
    public EmbedBlockLink() { }

    public Guid EmbedBlockId { get; set; }

    [JsonIgnore]
    public EmbedBlock EmbedBlock { get; set; }
}

public class DataBlock : ContentBlock
{
    public DataBlock() { }

    public string Heading { get; set; }

    public string Name { get; set; }

    public string? Source { get; set; }

    public FullTableQuery Query { get; set; }

    [JsonIgnore]
    public List<IChart> Charts
    {
        get
        {
            return ChartsInternal
                .Select(chart =>
                {
                    if (chart.Title.IsNullOrEmpty())
                    {
                        chart.Title = Heading;
                    }
                    return chart;
                })
                .ToList();
        }
        set => ChartsInternal = value;
    }

    // NOTE: We serialize ChartsInternal into JSON rather than Charts so that a chart title is set to null in the
    // database JSON. If we serialized Charts, then the serialization would run through Chart's getter, and so set
    // a chart title that is identical to the table heading. So to keep the database-stored chart JSON pure, we set
    // this JsonProperty, preventing the need to migrate existing charts, and Chart's getter will provide the table
    // heading in request responses when necessary.
    [JsonProperty("Charts")]
    [NotMapped]
    private List<IChart> ChartsInternal { get; set; } = new();

    public TableBuilderConfiguration Table { get; set; }

    [JsonIgnore]
    public DataBlockVersion DataBlockVersion { get; set; } = null!;
}

[AttributeUsage(AttributeTargets.Field)]
public class ContentBlockClassType : Attribute
{
    public Type Type { get; set; } = null!;
}

public enum ContentBlockType
{
    [ContentBlockClassType(Type = typeof(HtmlBlock))]
    HtmlBlock,
}

public static class ContentBlockUtil
{
    public static Type GetContentBlockClassTypeFromEnumValue(ContentBlockType enumValue)
    {
        return enumValue.GetEnumAttribute<ContentBlockClassType>().Type;
    }
}
