using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required List<FilterMeta> Filters { get; set; }

    public required List<IndicatorMeta> Indicators { get; set; }

    public required List<TimePeriodMeta> TimePeriods { get; set; }

    public required List<LocationMeta> Locations { get; set; }

    public required List<GeographicLevel> GeographicLevels { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<DataSetMeta>
    {
        public void Configure(EntityTypeBuilder<DataSetMeta> builder)
        {
            builder.Property(m => m.GeographicLevels).HasColumnType("text[]");

            builder.OwnsMany(m => m.Filters, m =>
            {
                m.ToJson();
                m.Property(fm => fm.Identifier).HasJsonPropertyName("Id");
                m.OwnsMany(fm => fm.Options);
            })
            .OwnsMany(m => m.Indicators, m =>
            {
                m.ToJson();
                m.Property(im => im.Identifier).HasJsonPropertyName("Id");
                m.Property(im => im.Unit)
                    .HasConversion(new EnumToEnumValueConverter<IndicatorUnit>());
            })
            .OwnsMany(m => m.TimePeriods, m =>
            {
                m.ToJson();
                m.Property(tpm => tpm.Code)
                    .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>());
            })
            .OwnsMany(m => m.Locations, m =>
            {
                m.ToJson();
                m.OwnsMany(lm => lm.Options);
            });
        }
    }
}
