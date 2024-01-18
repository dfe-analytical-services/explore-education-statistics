using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class ChangeSet<TChangeState> : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required Guid DataSetVersionId { get; set; }

    public required List<Change<TChangeState>> Changes { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }
}

public class ChangeSetFilters : ChangeSet<FilterChangeState>
{
    internal class Config : IEntityTypeConfiguration<ChangeSetFilters>
    {
        public void Configure(EntityTypeBuilder<ChangeSetFilters> builder)
        {
            builder.OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Identifier).HasJsonPropertyName("Id");
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState);
                cs.OwnsOne(c => c.PreviousState);
            });
        }
    }
}

public class ChangeSetFilterOptions : ChangeSet<FilterOptionChangeState>
{
    internal class Config : IEntityTypeConfiguration<ChangeSetFilterOptions>
    {
        public void Configure(EntityTypeBuilder<ChangeSetFilterOptions> builder)
        {
            builder.OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Identifier).HasJsonPropertyName("Id");
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState);
                cs.OwnsOne(c => c.PreviousState);
            });
        }
    }
}

public class ChangeSetIndicators : ChangeSet<IndicatorChangeState>
{
    public class Config : IEntityTypeConfiguration<ChangeSetIndicators>
    {
        public void Configure(EntityTypeBuilder<ChangeSetIndicators> builder)
        {
            builder.OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Identifier).HasJsonPropertyName("Id");
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState, s =>
                {
                    s.Property(sb => sb.Unit)
                        .HasConversion(new EnumToEnumValueConverter<IndicatorUnit>());
                });
                cs.OwnsOne(c => c.PreviousState, s =>
                {
                    s.Property(sb => sb.Unit)
                        .HasConversion(new EnumToEnumValueConverter<IndicatorUnit>());
                });
            });
        }
    }
}

public class ChangeSetLocations : ChangeSet<LocationChangeState>
{
    internal class Config : IEntityTypeConfiguration<ChangeSetLocations>
    {
        public void Configure(EntityTypeBuilder<ChangeSetLocations> builder)
        {
            builder.OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Identifier).HasJsonPropertyName("Id");
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState, s =>
                {
                    s.Property(sb => sb.Level).HasConversion<string>();
                });
                cs.OwnsOne(c => c.PreviousState);
            });
        }
    }
}

public class ChangeSetTimePeriods : ChangeSet<TimePeriodChangeState>
{
    internal class Config : IEntityTypeConfiguration<ChangeSetTimePeriods>
    {
        public void Configure(EntityTypeBuilder<ChangeSetTimePeriods> builder)
        {
            builder.OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Identifier).HasJsonPropertyName("Id");
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState, s =>
                {
                    s.Property(sb => sb.Code)
                        .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>());
                });
                cs.OwnsOne(c => c.PreviousState, s =>
                {
                    s.Property(sb => sb.Code)
                        .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>());
                });
            });
        }
    }
}
