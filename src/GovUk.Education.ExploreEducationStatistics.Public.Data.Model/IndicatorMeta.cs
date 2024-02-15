using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class IndicatorMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public List<IndicatorOptionMeta> Options { get; set; } = [];

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<IndicatorMeta>
    {
        public void Configure(EntityTypeBuilder<IndicatorMeta> builder)
        {
            builder.OwnsMany(m => m.Options, m =>
            {
                m.ToJson();
                m.Property(o => o.Identifier).HasJsonPropertyName("Id");
                m.Property(o => o.Unit).HasConversion(new EnumToEnumValueConverter<IndicatorUnit>());
            });
        }
    }
}
