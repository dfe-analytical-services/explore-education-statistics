using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class Change<TEntity>
{
    public long Id { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required Guid DataSetVersionId { get; set; }

    public TEntity? CurrentState { get; set; }

    public TEntity? PreviousState { get; set; }

    internal abstract class BaseConfig<TChange, TChangeEntity> : IEntityTypeConfiguration<TChange>
        where TChange : Change<TChangeEntity>
        where TChangeEntity : class
    {
        public virtual void Configure(EntityTypeBuilder<TChange> builder)
        {
            builder.HasOne(c => c.CurrentState).WithMany();

            builder.HasOne(c => c.PreviousState).WithMany();

            builder.Navigation(c => c.PreviousState).AutoInclude();
            builder.Navigation(c => c.CurrentState).AutoInclude();
        }
    }
}

public class FilterMetaChange : Change<FilterMeta>
{
    public int? CurrentStateId { get; set; }

    public int? PreviousStateId { get; set; }

    internal class Config : BaseConfig<FilterMetaChange, FilterMeta>;
}

public class FilterOptionMetaChange : Change<FilterOptionMetaChange.State>
{
    internal class Config : IEntityTypeConfiguration<FilterOptionMetaChange>
    {
        public void Configure(EntityTypeBuilder<FilterOptionMetaChange> builder)
        {
            builder.OwnsOne(
                c => c.PreviousState,
                b =>
                {
                    b.HasOne(s => s.Meta).WithMany();

                    b.HasOne(s => s.Option).WithMany();

                    b.Navigation(s => s.Meta).AutoInclude();
                    b.Navigation(s => s.Option).AutoInclude();
                }
            );

            builder.OwnsOne(
                c => c.CurrentState,
                b =>
                {
                    b.HasOne(s => s.Meta).WithMany();

                    b.HasOne(s => s.Option).WithMany();

                    b.Navigation(s => s.Meta).AutoInclude();
                    b.Navigation(s => s.Option).AutoInclude();
                }
            );
        }
    }

    public class State
    {
        public FilterMeta Meta { get; set; } = null!;

        public required int MetaId { get; set; }

        public FilterOptionMeta Option { get; set; } = null!;

        public required int OptionId { get; set; }

        public required string PublicId { get; set; }

        public static State Create(FilterOptionMetaLink link)
        {
            return new State
            {
                Meta = link.Meta,
                MetaId = link.MetaId,
                Option = link.Option,
                OptionId = link.OptionId,
                PublicId = link.PublicId,
            };
        }
    }
}

public class GeographicLevelMetaChange : Change<GeographicLevelMeta>
{
    public int? CurrentStateId { get; set; }

    public int? PreviousStateId { get; set; }

    internal class Config : BaseConfig<GeographicLevelMetaChange, GeographicLevelMeta>;
}

public class IndicatorMetaChange : Change<IndicatorMeta>
{
    public int? CurrentStateId { get; set; }

    public int? PreviousStateId { get; set; }

    internal class Config : BaseConfig<IndicatorMetaChange, IndicatorMeta>;
}

public class LocationMetaChange : Change<LocationMeta>
{
    public int? CurrentStateId { get; set; }

    public int? PreviousStateId { get; set; }

    internal class Config : BaseConfig<LocationMetaChange, LocationMeta>;
}

public class LocationOptionMetaChange : Change<LocationOptionMetaChange.State>
{
    internal class Config : IEntityTypeConfiguration<LocationOptionMetaChange>
    {
        public void Configure(EntityTypeBuilder<LocationOptionMetaChange> builder)
        {
            builder.OwnsOne(
                c => c.PreviousState,
                b =>
                {
                    b.HasOne(s => s.Meta).WithMany();

                    b.HasOne(s => s.Option).WithMany();

                    b.Navigation(s => s.Meta).AutoInclude();
                    b.Navigation(s => s.Option).AutoInclude();
                }
            );

            builder.OwnsOne(
                c => c.CurrentState,
                b =>
                {
                    b.HasOne(s => s.Meta).WithMany();

                    b.HasOne(s => s.Option).WithMany();

                    b.Navigation(s => s.Meta).AutoInclude();
                    b.Navigation(s => s.Option).AutoInclude();
                }
            );
        }
    }

    public class State
    {
        public LocationMeta Meta { get; set; } = null!;

        public required int MetaId { get; set; }

        public LocationOptionMeta Option { get; set; } = null!;

        public required int OptionId { get; set; }

        public required string PublicId { get; set; }

        public static State Create(LocationOptionMetaLink link)
        {
            return new State
            {
                Meta = link.Meta,
                MetaId = link.MetaId,
                Option = link.Option,
                OptionId = link.OptionId,
                PublicId = link.PublicId,
            };
        }
    }
}

public class TimePeriodMetaChange : Change<TimePeriodMeta>
{
    public int? CurrentStateId { get; set; }

    public int? PreviousStateId { get; set; }

    internal class Config : BaseConfig<TimePeriodMetaChange, TimePeriodMeta>;
}
