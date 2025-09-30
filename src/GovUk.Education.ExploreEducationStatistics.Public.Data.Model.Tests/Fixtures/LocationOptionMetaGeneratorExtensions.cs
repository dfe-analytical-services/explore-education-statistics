using System.Linq.Expressions;
using Bogus;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationOptionMetaGeneratorExtensions
{
    public static Generator<TOptionMeta> WithId<TOptionMeta>(
        this Generator<TOptionMeta> generator,
        int id
    )
        where TOptionMeta : LocationOptionMeta => generator.ForInstance(s => s.SetId(id));

    public static Generator<TOptionMeta> WithLabel<TOptionMeta>(
        this Generator<TOptionMeta> generator,
        string label
    )
        where TOptionMeta : LocationOptionMeta => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<TOptionMeta> WithMetas<TOptionMeta>(
        this Generator<TOptionMeta> generator,
        Func<IEnumerable<LocationMeta>> metas
    )
        where TOptionMeta : LocationOptionMeta => generator.ForInstance(s => s.SetMetas(metas));

    public static Generator<TOptionMeta> WithMetaLinks<TOptionMeta>(
        this Generator<TOptionMeta> generator,
        Func<IEnumerable<LocationOptionMetaLink>> links
    )
        where TOptionMeta : LocationOptionMeta => generator.ForInstance(s => s.SetMetaLinks(links));

    public static InstanceSetters<TOptionMeta> SetBaseDefaults<TOptionMeta>(
        this InstanceSetters<TOptionMeta> setters
    )
        where TOptionMeta : LocationOptionMeta => setters.SetDefault(m => m.Label);

    public static InstanceSetters<TOptionMeta> SetDefaultCode<TOptionMeta>(
        this InstanceSetters<TOptionMeta> setters,
        Expression<Func<TOptionMeta, string?>> property
    )
        where TOptionMeta : LocationOptionMeta =>
        setters.Set(
            property,
            (faker, _, context) =>
            {
                var index = setters.GetDisplayIndex(context, faker);
                return $"Option {index} {PropertyName.For(property)}";
            }
        );

    public static InstanceSetters<TOptionMeta> SetId<TOptionMeta>(
        this InstanceSetters<TOptionMeta> setters,
        int id
    )
        where TOptionMeta : LocationOptionMeta => setters.Set(m => m.Id, id);

    public static InstanceSetters<TOptionMeta> SetLabel<TOptionMeta>(
        this InstanceSetters<TOptionMeta> setters,
        string label
    )
        where TOptionMeta : LocationOptionMeta => setters.Set(m => m.Label, label);

    public static InstanceSetters<TOptionMeta> SetMetas<TOptionMeta>(
        this InstanceSetters<TOptionMeta> setters,
        Func<IEnumerable<LocationMeta>> metas
    )
        where TOptionMeta : LocationOptionMeta => setters.Set(m => m.Metas, metas);

    public static InstanceSetters<TOptionMeta> SetMetaLinks<TOptionMeta>(
        this InstanceSetters<TOptionMeta> setters,
        Func<IEnumerable<LocationOptionMetaLink>> links
    )
        where TOptionMeta : LocationOptionMeta => setters.Set(m => m.MetaLinks, links);
}
