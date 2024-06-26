using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class Change<TEntity, TIdentifier>
{
    public long Id { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required Guid DataSetVersionId { get; set; }

    public TEntity? CurrentState { get; set; }

    public TIdentifier? CurrentStateId { get; set; }

    public TEntity? PreviousState { get; set; }

    public TIdentifier? PreviousStateId { get; set; }

    internal abstract class BaseConfig<TChange, TChangeEntity> : IEntityTypeConfiguration<TChange>
        where TChange : Change<TChangeEntity, TIdentifier>
        where TChangeEntity : class
    {
        public virtual void Configure(EntityTypeBuilder<TChange> builder)
        {
            builder
                .HasOne(c => c.CurrentState)
                .WithMany()
                .IsRequired(false);

            builder
                .HasOne(c => c.PreviousState)
                .WithMany()
                .IsRequired(false);

            builder.Navigation(c => c.PreviousState).AutoInclude();
            builder.Navigation(c => c.CurrentState).AutoInclude();
        }
    }
}

public class FilterMetaChange : Change<FilterMeta, int?>
{
    internal class Config : BaseConfig<FilterMetaChange, FilterMeta>;
}

public class FilterOptionMetaChange : Change<FilterOptionMeta, int?>
{
    public required string PublicId { get; set; }

    public FilterMeta Meta { get; set; } = null!;

    public required int MetaId { get; set; }

    internal class Config : BaseConfig<FilterOptionMetaChange, FilterOptionMeta>
    {
        public override void Configure(EntityTypeBuilder<FilterOptionMetaChange> builder)
        {
            base.Configure(builder);

            builder.HasIndex(c => new { c.MetaId, c.PublicId })
                .IsUnique();
        }
    }
}

public class GeographicLevelMetaChange : Change<GeographicLevelMeta, int?>
{
    internal class Config : BaseConfig<GeographicLevelMetaChange, GeographicLevelMeta>;
}

public class IndicatorMetaChange : Change<IndicatorMeta, int?>
{
    internal class Config : BaseConfig<IndicatorMetaChange, IndicatorMeta>;
}

public class LocationMetaChange : Change<LocationMeta, int?>
{
    internal class Config : BaseConfig<LocationMetaChange, LocationMeta>;
}

public class LocationOptionMetaChange : Change<LocationOptionMeta, int?>
{
    public LocationMeta Meta { get; set; } = null!;

    public int MetaId { get; set; }

    internal class Config : BaseConfig<LocationOptionMetaChange, LocationOptionMeta>;
}

public class TimePeriodMetaChange : Change<TimePeriodMeta, int?>
{
    internal class Config : BaseConfig<TimePeriodMetaChange, TimePeriodMeta>;
}
