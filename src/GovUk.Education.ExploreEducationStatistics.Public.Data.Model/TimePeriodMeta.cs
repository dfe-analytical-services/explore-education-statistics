using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class TimePeriodMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public int Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required TimeIdentifier Code { get; set; }

    /// <summary>
    /// In yyyy/yyyy format i.e. 2020/2021.
    /// </summary>
    public required string Period { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<TimePeriodMeta>
    {
        public void Configure(EntityTypeBuilder<TimePeriodMeta> builder)
        {
            builder.Property(m => m.Period)
                .HasMaxLength(9);
            
            builder.Property(m => m.Code)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(10);

            builder.HasIndex(m => new { m.DataSetVersionId, m.Code, m.Period })
                .IsUnique();
        }
    }
}
