using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class IndicatorMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public int Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required string PublicId { get; set; }

    public required string Column { get; set; }

    public required string Label { get; set; }

    public IndicatorUnit? Unit { get; set; }

    public byte? DecimalPlaces { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<IndicatorMeta>
    {
        public void Configure(EntityTypeBuilder<IndicatorMeta> builder)
        {
            builder.Property(m => m.PublicId)
                .HasMaxLength(10);

            builder.Property(m => m.Column)
                .HasMaxLength(50);

            builder.Property(m => m.Label)
                .HasMaxLength(100);

            builder.Property(m => m.Unit)
                .HasConversion(new EnumToEnumValueConverter<IndicatorUnit>());

            builder.HasIndex(m => new { m.DataSetVersionId, m.PublicId })
                .IsUnique();

            builder.HasIndex(m => new { m.DataSetVersionId, m.Column })
                .IsUnique();
        }
    }
}
