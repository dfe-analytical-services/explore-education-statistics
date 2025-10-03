using GovUk.Education.ExploreEducationStatistics.Common.Comparers;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class GeographicLevelMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public int Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required List<GeographicLevel> Levels { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<GeographicLevelMeta>
    {
        public void Configure(EntityTypeBuilder<GeographicLevelMeta> builder)
        {
            builder
                .Property(msb => msb.Levels)
                .HasColumnType("text[]")
                .HasConversion(
                    new EnumToEnumValueListConverter<GeographicLevel>(),
                    new ListValueComparer<GeographicLevel>()
                );
        }
    }
}
