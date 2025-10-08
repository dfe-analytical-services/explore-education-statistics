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

    public required GeographicLevel Level { get; set; }

    public List<LocationOptionMeta> Options { get; set; } = [];

    public List<LocationOptionMetaLink> OptionLinks { get; set; } = [];

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public static implicit operator GeographicLevel(LocationMeta meta) => meta.Level;

    internal class Config : IEntityTypeConfiguration<LocationMeta>
    {
        public void Configure(EntityTypeBuilder<LocationMeta> builder)
        {
            builder
                .HasMany(m => m.Options)
                .WithMany(o => o.Metas)
                .UsingEntity<LocationOptionMetaLink>(
                    b => b.HasOne(l => l.Option).WithMany(o => o.MetaLinks).HasForeignKey(l => l.OptionId),
                    b => b.HasOne(l => l.Meta).WithMany(m => m.OptionLinks).HasForeignKey(l => l.MetaId)
                );

            builder
                .Property(m => m.Level)
                .HasMaxLength(5)
                .HasConversion(new EnumToEnumValueConverter<GeographicLevel>());

            builder.HasIndex(m => new { m.DataSetVersionId, m.Level }).IsUnique();
        }
    }
}
