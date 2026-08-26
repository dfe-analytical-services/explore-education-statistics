#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using JsonKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

[JsonConverter(typeof(JsonKnownTypesConverter<ContentBlock>))]
[JsonDiscriminator(Name = "Type")]
[KnownType(typeof(DataBlockVersionLink))]
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
    public ContentSection ContentSection { get; set; } = null!;

    [JsonIgnore]
    public Guid ContentSectionId { get; set; }

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; }

    public int Order { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }

    public List<Comment> Comments { get; set; } = [];

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

    public string? Body { get; set; }
}

public class EmbedBlockLink : ContentBlock
{
    public EmbedBlockLink() { }

    public Guid EmbedBlockId { get; set; }

    [JsonIgnore]
    public EmbedBlock EmbedBlock { get; set; }
}

public class DataBlockVersionLink : ContentBlock
{
    public DataBlockVersionLink() { }

    public Guid DataBlockVersionId { get; set; }

    [JsonIgnore]
    public DataBlockVersion DataBlockVersion { get; set; } = null!;

    internal class Config : IEntityTypeConfiguration<DataBlockVersionLink>
    {
        public void Configure(EntityTypeBuilder<DataBlockVersionLink> builder)
        {
            // NoAction rather than the convention's Cascade for a required one-to-one. A ReleaseVersion
            // already cascades to this ContentBlock row via its ContentSection, so cascading from
            // DataBlockVersions as well would give SQL Server multiple cascade paths to the same row.
            // Callers deleting a DataBlockVersion must therefore remove its link explicitly.
            builder.HasOne(block => block.DataBlockVersion).WithOne().OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(block => block.DataBlockVersion).AutoInclude();
        }
    }
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
