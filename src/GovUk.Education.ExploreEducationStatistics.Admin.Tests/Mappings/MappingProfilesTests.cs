#nullable enable
using AutoMapper;
using AutoMapper.Internal;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Mappings;

/// <summary>
/// AutoMapper is affected by GHSA-rvv3-g6hj-g44x (CVE-2026-32933), where mapping a deeply nested source object
/// graph through a circular type map can cause an uncatchable stack overflow. Only circular type maps are
/// vulnerable - see https://github.com/LuckyPennySoftware/AutoMapper/discussions/4624.
///
/// The NuGet audit warning for this advisory is suppressed in the Admin project on the basis that the Admin
/// mapping configuration has no circular type maps. These tests provide continued assurance of that.
/// </summary>
public class MappingProfilesTests
{
    [Fact]
    public void AdminMappingProfiles_HaveNoCircularTypeMaps()
    {
        var configuration = MapperUtils.AdminMapper().ConfigurationProvider;

        var cycles = FindCircularMaps(configuration).Select(DescribeCycle).ToList();

        Assert.True(
            cycles.Count == 0,
            $"""
            Found circular AutoMapper type maps, which are vulnerable to GHSA-rvv3-g6hj-g44x
            (https://github.com/LuckyPennySoftware/AutoMapper/discussions/4624):
            {string.Join(Environment.NewLine, cycles)}
            Either remove the circular reference, or set MaxDepth on the map.
            """
        );
    }

    [Fact]
    public void FindCircularMaps_DirectSelfReference_IsReported()
    {
        var configuration = new MapperConfiguration(cfg => cfg.CreateMap<SelfRef, SelfRefViewModel>());

        Assert.Equal(
            ["SelfRef -> SelfRefViewModel => SelfRef -> SelfRefViewModel"],
            FindCircularMaps(configuration).Select(DescribeCycle)
        );
    }

    [Fact]
    public void FindCircularMaps_IndirectReference_IsReported()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Parent, ParentViewModel>();
            cfg.CreateMap<Child, ChildViewModel>();
        });

        Assert.Equal(
            [
                "Child -> ChildViewModel => Parent -> ParentViewModel => Child -> ChildViewModel",
                "Parent -> ParentViewModel => Child -> ChildViewModel => Parent -> ParentViewModel",
            ],
            FindCircularMaps(configuration).Select(DescribeCycle).Order()
        );
    }

    [Fact]
    public void FindCircularMaps_ReferenceThroughCollection_IsReported()
    {
        var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Node, NodeViewModel>());

        Assert.Equal(
            ["Node -> NodeViewModel => Node -> NodeViewModel"],
            FindCircularMaps(configuration).Select(DescribeCycle)
        );
    }

    [Fact]
    public void FindCircularMaps_ReferenceThroughIncludedDerivedType_IsReported()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Base, BaseViewModel>().IncludeAllDerived();
            cfg.CreateMap<Derived, DerivedViewModel>();
        });

        Assert.Equal(
            [
                "Base -> BaseViewModel => Derived -> DerivedViewModel => Base -> BaseViewModel",
                "Derived -> DerivedViewModel => Base -> BaseViewModel => Derived -> DerivedViewModel",
            ],
            FindCircularMaps(configuration).Select(DescribeCycle).Order()
        );
    }

    [Fact]
    public void FindCircularMaps_NoCircularReference_IsNotReported()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Branch, BranchViewModel>();
            cfg.CreateMap<Leaf, LeafViewModel>();
        });

        Assert.Empty(FindCircularMaps(configuration));
    }

    // Adapted from the maintainer's FindCircularMaps / IsCircular in
    // https://github.com/LuckyPennySoftware/AutoMapper/discussions/4624
    // which uses AutoMapper's metadata to detect circular maps.
    //
    // The code in that discussion is extended to:
    //  1. follow collection members via their element type. The original looks up type maps by the member's own
    //     destination type (e.g. List<NodeViewModel>), which never has a type map, so it misses cycles through
    //     collections.
    //  2. follow included derived type maps (e.g. ContentBlock -> IContentBlockViewModel uses IncludeAllDerived()).
    //
    // IsCircular is renamed FindCycle as it returns the cycle path rather than a bool, for reporting.
    private static IEnumerable<List<TypeMap>> FindCircularMaps(IConfigurationProvider config)
    {
        var allMaps = config.Internal().GetAllTypeMaps();
        var mapsByDestType = allMaps.ToLookup(tm => tm.DestinationType);
        var mapsByTypes = allMaps.ToDictionary(tm => tm.Types); // Extension 2

        return allMaps.Select(tm => FindCycle(tm, mapsByDestType, mapsByTypes, [])).OfType<List<TypeMap>>();
    }

    private static List<TypeMap>? FindCycle(
        TypeMap current,
        ILookup<Type, TypeMap> mapsByDestType,
        Dictionary<TypePair, TypeMap> mapsByTypes,
        List<TypeMap> path
    )
    {
        if (path.Contains(current))
        {
            return [.. path, current];
        }

        path = [.. path, current];

        var nextMaps = current
            .MemberMaps.Where(m => !m.Ignored)
            .SelectMany(mm => mapsByDestType[UnwrapElementType(mm.DestinationType)]) // Extension 1
            .Concat(current.IncludedDerivedTypes.Where(mapsByTypes.ContainsKey).Select(t => mapsByTypes[t])); // Extension 2

        return nextMaps
            .Select(nextMap => FindCycle(nextMap, mapsByDestType, mapsByTypes, path))
            .OfType<List<TypeMap>>()
            .FirstOrDefault();
    }

    private static Type UnwrapElementType(Type type)
    {
        if (type == typeof(string))
        {
            return type;
        }

        if (type.IsArray)
        {
            return type.GetElementType()!;
        }

        var enumerableType =
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type
                : type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableType?.GetGenericArguments()[0] ?? type;
    }

    private static string DescribeCycle(List<TypeMap> cycle) =>
        string.Join(" => ", cycle.Select(tm => $"{tm.SourceType.Name} -> {tm.DestinationType.Name}"));

    private class SelfRef
    {
        public SelfRef? Next { get; set; }
    }

    private class SelfRefViewModel
    {
        public SelfRefViewModel? Next { get; set; }
    }

    private class Parent
    {
        public Child? Child { get; set; }
    }

    private class ParentViewModel
    {
        public ChildViewModel? Child { get; set; }
    }

    private class Child
    {
        public Parent? Parent { get; set; }
    }

    private class ChildViewModel
    {
        public ParentViewModel? Parent { get; set; }
    }

    private class Node
    {
        public List<Node> Children { get; set; } = [];
    }

    private class NodeViewModel
    {
        public List<NodeViewModel> Children { get; set; } = [];
    }

    private class Base
    {
        public int Id { get; set; }
    }

    private class BaseViewModel
    {
        public int Id { get; set; }
    }

    private class Derived : Base
    {
        public Base? Inner { get; set; }
    }

    private class DerivedViewModel : BaseViewModel
    {
        public BaseViewModel? Inner { get; set; }
    }

    private class Branch
    {
        public List<Leaf> Leaves { get; set; } = [];
    }

    private class BranchViewModel
    {
        public List<LeafViewModel> Leaves { get; set; } = [];
    }

    private class Leaf
    {
        public string Name { get; set; } = "";
    }

    private class LeafViewModel
    {
        public string Name { get; set; } = "";
    }
}
