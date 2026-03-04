#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ContentSection
{
    public Guid Id { get; init; }

    public int Order { get; set; }

    public string? Heading { get; set; }

    public List<ContentBlock> Content { get; set; } = [];

    public ReleaseVersion ReleaseVersion { get; init; } = null!;

    public Guid ReleaseVersionId { get; init; }

    [JsonIgnore]
    public ContentSectionType Type { get; init; } = ContentSectionType.Generic;

    public T? FindSingleContentBlockOfType<T>()
        where T : ContentBlock => Content.OfType<T>().SingleOrDefault();

    internal class Config : IEntityTypeConfiguration<ContentSection>
    {
        public void Configure(EntityTypeBuilder<ContentSection> builder)
        {
            builder
                .Property(cs => cs.Type)
                .HasConversion(new EnumToStringConverter<ContentSectionType>())
                .HasMaxLength(25);

            builder.HasIndex(cs => cs.Type);

            // Unique constraint for all section types except for 'Generic' which can occur multiple times per release version.
            builder.HasIndex(cs => new { cs.ReleaseVersionId, cs.Type }).IsUnique().HasFilter("[Type] <> 'Generic'");
        }
    }
}
