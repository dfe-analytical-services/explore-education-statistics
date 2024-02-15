using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class GeographicLevelMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    private string[] _options = [];

    public required HashSet<GeographicLevel> Options
    {
        get => _options
            .Select(EnumToEnumValueConverter<GeographicLevel>.FromProvider)
            .ToHashSet();
        set => _options = value
            .Select(EnumToEnumValueConverter<GeographicLevel>.ToProvider)
            .ToArray();
    }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<GeographicLevelMeta>
    {
        public void Configure(EntityTypeBuilder<GeographicLevelMeta> builder)
        {
            builder
                .Property(m => m._options)
                .HasColumnType("text[]")
                .HasColumnName(nameof(Options));

            builder.Ignore(m => m.Options);
        }
    }
}
