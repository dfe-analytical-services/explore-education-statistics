using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public int Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public GeographicLevel Level { get; set; }

    public List<LocationOptionMeta> Options { get; set; } = [];

    public List<LocationOptionMetaLink> OptionLinks { get; set; } = [];

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<LocationMeta>
    {
        public void Configure(EntityTypeBuilder<LocationMeta> builder)
        {
            builder.HasMany<LocationOptionMeta>(m => m.Options)
                .WithMany(o => o.Metas)
                .UsingEntity<LocationOptionMetaLink>(
                    b => b
                        .HasOne<LocationOptionMeta>(l => l.Option)
                        .WithMany(o => o.MetaLinks)
                        .HasForeignKey(l => l.OptionId),
                    b => b
                        .HasOne<LocationMeta>(l => l.Meta)
                        .WithMany(m => m.OptionLinks)
                        .HasForeignKey(l => l.MetaId)
                );

            builder.Property(m => m.Level)
                .HasMaxLength(5)
                .HasConversion(new EnumToEnumValueConverter<GeographicLevel>());

            builder.HasIndex(m => new {m.Level, m.DataSetVersionId})
                .IsUnique();
        }
    }
}
