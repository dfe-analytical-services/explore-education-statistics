#nullable enable
using System;
using System.Linq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

public class GeneratorTests
{
    private class Test
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    [Fact]
    public void Generate_Single()
    {
        var item = new Generator<Test>()
            .ForInstance(s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Doe"))
            .Generate();

        Assert.Equal("Jane", item.FirstName);
        Assert.Equal("Doe", item.LastName);
    }

    [Fact]
    public void Generate_Single_SetDefaults()
    {
        var item = new Generator<Test>()
            .ForInstance(s => s
                .SetDefault(t => t.FirstName)
                .SetDefault(t => t.LastName))
            .Generate();

        Assert.Equal("FirstName of Test 0", item.FirstName);
        Assert.Equal("LastName of Test 0", item.LastName);
    }

    [Fact]
    public void Generate_Single_LastSetterOverrides()
    {
        var item = new Generator<Test>()
            .ForInstance(s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Doe")
                .Set(t => t.LastName, "Loe"))
            .Generate();

        Assert.Equal("Jane", item.FirstName);
        Assert.Equal("Loe", item.LastName);
    }

    [Fact]
    public void Generate_Single_Random()
    {
        var item = new Generator<Test>(seeder: () => 0)
            .ForInstance(s => s
                .Set(t => t.FirstName, faker => faker.Name.FirstName())
                .Set(t => t.LastName, faker => faker.Name.LastName()))
            .Generate();

        Assert.Equal("Moises", item.FirstName);
        Assert.Equal("Schumm", item.LastName);
    }

    [Fact]
    public void Generate_Single_IndexIsZero()
    {
        var item = new Generator<Test>()
            .ForInstance(s => s
                .Set(t => t.FirstName, (_, _, context) => $"Jane {context.Index}")
                .Set(t => t.LastName, (_, _, context) => $"Doe {context.Index}"))
            .Generate();

        Assert.Equal("Jane 0", item.FirstName);
        Assert.Equal("Doe 0", item.LastName);
    }

    [Fact]
    public void Generate_Single_InstanceSetter()
    {
        var item = new Generator<Test>()
            .ForInstance(s => s
                .Set((_, t, _) =>
                    {
                        t.FirstName = "John";
                        t.LastName = "Doe";
                    }
                ))
            .Generate();

        Assert.Equal("John", item.FirstName);
        Assert.Equal("Doe", item.LastName);
    }

    [Fact]
    public void Generate_Single_InstantiateWith()
    {
        var item = new Generator<Test>()
            .InstantiateWith(
                () => new Test
                {
                    FirstName = "Jill",
                    LastName = "Roe"
                }
            )
            .Generate();

        Assert.Equal("Jill", item.FirstName);
        Assert.Equal("Roe", item.LastName);
    }

    [Fact]
    public void Generate_Single_InstantiateWith_Setter()
    {
        var item = new Generator<Test>()
            .InstantiateWith(
                () => new Test
                {
                    FirstName = "Jill",
                    LastName = "Roe"
                }
            )
            .ForInstance(s => s
                .Set(t => t.FirstName, "Jane"))
            .Generate();

        Assert.Equal("Jane", item.FirstName);
        Assert.Equal("Roe", item.LastName);
    }

    [Fact]
    public void Generate_Single_NoSetters()
    {
        var item = new Generator<Test>().Generate();

        Assert.Equal("", item.FirstName);
        Assert.Equal("", item.LastName);
    }

    [Fact]
    public void Generate_Multiple()
    {
        var items = new Generator<Test>()
            .ForInstance(s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Doe"))
            .GenerateList(3);

        Assert.All(items, item => Assert.Equal("Jane", item.FirstName));
        Assert.All(items, item => Assert.Equal("Doe", item.LastName));
    }

    [Fact]
    public void Generate_Multiple_SetDefaults()
    {
        var items = new Generator<Test>()
            .ForInstance(s => s
                .SetDefault(t => t.FirstName)
                .SetDefault(t => t.LastName))
            .GenerateList(3);

        Assert.Equal("FirstName of Test 0", items[0].FirstName);
        Assert.Equal("LastName of Test 0", items[0].LastName);
        Assert.Equal("FirstName of Test 1", items[1].FirstName);
        Assert.Equal("LastName of Test 1", items[1].LastName);
        Assert.Equal("FirstName of Test 2", items[2].FirstName);
        Assert.Equal("LastName of Test 2", items[2].LastName);
    }

    [Fact]
    public void Generate_Multiple_LastSetterOverrides()
    {
        var items = new Generator<Test>()
            .ForInstance(s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Doe")
                .Set(t => t.LastName, "Loe"))
            .GenerateList(3);

        Assert.All(items, item => Assert.Equal("Jane", item.FirstName));
        Assert.All(items, item => Assert.Equal("Loe", item.LastName));
    }

    [Fact]
    public void Generate_Multiple_IndexIncrementCorrectly()
    {
        var items = new Generator<Test>()
            .ForInstance(s => s
                .Set(t => t.FirstName, (_, _, context) => $"Jane {context.Index}")
                .Set(t => t.LastName, (_, _, context) => $"Doe {context.Index}"))
            .GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Doe 0", items[0].LastName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Doe 1", items[1].LastName);
        Assert.Equal("Jane 2", items[2].FirstName);
        Assert.Equal("Doe 2", items[2].LastName);
    }

    [Fact]
    public void Generate_Multiple_InstanceSetter()
    {
        var items = new Generator<Test>()
            .ForInstance(s => s
                .Set((_, t, _) =>
                    {
                        t.FirstName = "John";
                        t.LastName = "Doe";
                    }
                ))
            .GenerateList(3);

        Assert.All(items, item => Assert.Equal("John", item.FirstName));
        Assert.All(items, item => Assert.Equal("Doe", item.LastName));
    }

    [Fact]
    public void Generate_Multiple_InstantiateWith()
    {
        var items = new Generator<Test>()
            .InstantiateWith(
                () => new Test
                {
                    FirstName = "Jill",
                    LastName = "Roe"
                }
            )
            .GenerateList(3);

        Assert.All(items, item => Assert.Equal("Jill", item.FirstName));
        Assert.All(items, item => Assert.Equal("Roe", item.LastName));
    }

    [Fact]
    public void Generate_Multiple_InstantiateWith_Setter()
    {
        var items = new Generator<Test>()
            .InstantiateWith(
                () => new Test
                {
                    FirstName = "Jill",
                    LastName = "Roe"
                }
            )
            .ForInstance(s => s
                .Set(t => t.FirstName, "Jane"))
            .GenerateList(3);

        Assert.All(items, item => Assert.Equal("Jane", item.FirstName));
        Assert.All(items, item => Assert.Equal("Roe", item.LastName));
    }

    [Fact]
    public void Generate_Multiple_IndexFakerIncrementsCorrectly()
    {
        var generator = new Generator<Test>()
            .ForInstance(
                s => s
                    .Set(t => t.FirstName, faker => $"Jane {faker.IndexFaker}")
            );

        var items = generator.GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);

        items = generator.GenerateList(3);

        Assert.Equal("Jane 3", items[0].FirstName);
        Assert.Equal("Jane 4", items[1].FirstName);
        Assert.Equal("Jane 5", items[2].FirstName);
    }

    [Fact]
    public void Generate_Multiple_NoSetters()
    {
        var items = new Generator<Test>().GenerateList(3);

        Assert.All(items, item => Assert.Equal("", item.FirstName));
        Assert.All(items, item => Assert.Equal("", item.LastName));
    }

    [Fact]
    public void Generate_Multiple_Random_SameSeed()
    {
        var items = new Generator<Test>(seeder: () => 1)
            .ForInstance(s => s
                .Set(t => t.FirstName, faker => faker.Name.FirstName())
                .Set(t => t.LastName, faker => faker.Name.LastName()))
            .GenerateList(3);

        Assert.Equal("Delores", items[0].FirstName);
        Assert.Equal("Brown", items[0].LastName);
        Assert.Equal("Delores", items[1].FirstName);
        Assert.Equal("Brown", items[1].LastName);
        Assert.Equal("Delores", items[2].FirstName);
        Assert.Equal("Brown", items[2].LastName);
    }

    [Fact]
    public void Generate_Multiple_Random_IncrementingSeed()
    {
        var seed = 0;
        var items = new Generator<Test>(seeder: () => seed++)
            .ForInstance(s => s
                .Set(t => t.FirstName, faker => faker.Name.FirstName())
                .Set(t => t.LastName, faker => faker.Name.LastName()))
            .GenerateList(3);

        Assert.Equal("Moises", items[0].FirstName);
        Assert.Equal("Schumm", items[0].LastName);
        Assert.Equal("Delores", items[1].FirstName);
        Assert.Equal("Brown", items[1].LastName);
        Assert.Equal("Ole", items[2].FirstName);
        Assert.Equal("Jakubowski", items[2].LastName);
    }

    [Fact]
    public void Generate_ForRange_Single()
    {
        var items = new Generator<Test>()
            .ForRange(..2, s => s
                .Set(t => t.FirstName, "John")
                .Set(t => t.LastName, "Doe"))
            .GenerateArray(2);

        Assert.Equal(2, items.Length);
        Assert.All(items[..2], item => Assert.Equal("John", item.FirstName));
        Assert.All(items[..2], item => Assert.Equal("Doe", item.LastName));
    }

    [Fact]
    public void Generate_ForRange_Single_NoOutOfRangeSetters()
    {
        var items = new Generator<Test>()
            .ForRange(..^2, s => s
                .Set(t => t.FirstName, "John")
                .Set(t => t.LastName, "Doe"))
            .GenerateArray(4);

        Assert.Equal(4, items.Length);
        Assert.All(items[..2], item => Assert.Equal("John", item.FirstName));
        Assert.All(items[..2], item => Assert.Equal("Doe", item.LastName));
        Assert.All(items[2..4], item => Assert.Equal("", item.FirstName));
        Assert.All(items[2..4], item => Assert.Equal("", item.LastName));
    }

    [Fact]
    public void Generate_ForRange_Multiple()
    {
        var items = new Generator<Test>()
            .ForRange(..2, s => s
                .Set(t => t.FirstName, "John")
                .Set(t => t.LastName, "Doe"))
            .ForRange(2..4, s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Loe"))
            .ForRange(4.., s => s
                .Set(t => t.FirstName, "Jill")
                .Set(t => t. LastName, "Roe"))
            .GenerateArray(6);

        Assert.Equal(6, items.Length);
        Assert.All(items[..2], item => Assert.Equal("John", item.FirstName));
        Assert.All(items[..2], item => Assert.Equal("Doe", item.LastName));
        Assert.All(items[2..4], item => Assert.Equal("Jane", item.FirstName));
        Assert.All(items[2..4], item => Assert.Equal("Loe", item.LastName));
        Assert.All(items[4..], item => Assert.Equal("Jill", item.FirstName));
        Assert.All(items[4..], item => Assert.Equal("Roe", item.LastName));
    }

    [Fact]
    public void Generate_ForRange_Multiple_Overlapping()
    {
        var items = new Generator<Test>()
            .ForRange(..2, s => s
                .Set(t => t.FirstName, "John")
                .Set(t => t.LastName, "Doe"))
            .ForRange(2..4, s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Loe"))
            .ForRange(3.., s => s
                .Set(t => t.FirstName, "Jill")
                .Set(t => t. LastName, "Roe"))
            .GenerateArray(6);

        Assert.Equal(6, items.Length);
        Assert.All(items[..2], item => Assert.Equal("John", item.FirstName));
        Assert.All(items[..2], item => Assert.Equal("Doe", item.LastName));

        Assert.Equal("Jane", items[2].FirstName);
        Assert.Equal("Loe", items[2].LastName);

        Assert.All(items[3..], item => Assert.Equal("Jill", item.FirstName));
        Assert.All(items[3..], item => Assert.Equal("Roe", item.LastName));
    }

    [Fact]
    public void Generate_ForRange_SameRanges_LastOverrides()
    {
        var items = new Generator<Test>()
            .ForRange(..2, s => s
                .Set(t => t.FirstName, "John")
                .Set(t => t.LastName, "Doe"))
            .ForRange(2..4, s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Loe"))
            .ForRange(..2, s => s
                .Set(t => t.FirstName, "Jill")
                .Set(t => t. LastName, "Roe"))
            .GenerateArray(4);

        Assert.Equal(4, items.Length);
        Assert.All(items[..2], item => Assert.Equal("Jill", item.FirstName));
        Assert.All(items[..2], item => Assert.Equal("Roe", item.LastName));
        Assert.All(items[2..4], item => Assert.Equal("Jane", item.FirstName));
        Assert.All(items[2..4], item => Assert.Equal("Loe", item.LastName));
    }

    [Fact]
    public void Generate_ForRange_RangeOutOfBoundsThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Generator<Test>()
                .ForRange(..2, s => s
                    .Set(t => t.FirstName, "John")
                    .Set(t => t.LastName, "Doe"))
                .ForRange(2..4, s => s
                    .Set(t => t.FirstName, "Jane")
                    .Set(t => t.LastName, "Loe"))
                .GenerateArray(2)
        );
    }

    [Fact]
    public void Generate_Multiple_GenerateList_NoArg()
    {
       var items = new Generator<Test>()
            .ForRange(..2, s => s
                .Set(t => t.FirstName, "John"))
            .ForRange(2..4, s => s
                .Set(t => t.FirstName, "Jane"))
            .GenerateList();
       
        Assert.Equal(4, items.Count);
    }

    [Fact]
    public void Generate_Multiple_GenerateArray_NoArg()
    {
        var items = new Generator<Test>()
            .ForRange(..2, s => s
                .Set(t => t.FirstName, "John"))
            .ForRange(2..4, s => s
                .Set(t => t.FirstName, "Jane"))
            .GenerateArray();
       
        Assert.Equal(4, items.Length);
    }
    
    [Fact]
    public void Generate_Multiple_GenerateArray_NoArg_IndexFromEndRangeSetterProvided()
    {
        var ex = Assert.Throws<ArgumentException>(() => 
            new Generator<Test>()
                .ForRange(..^2, s => s.Set(t => t.FirstName, "John"))
                .GenerateList());
        
        Assert.StartsWith(
            "Cannot infer number of elements to create if index-from-end", 
            ex.Message);
    }
    
    [Fact]
    public void Generate_Multiple_GenerateArray_NoArg_UnboundedMaximumSetterProvided()
    {
        var ex = Assert.Throws<ArgumentException>(() => 
            new Generator<Test>()
                .ForRange(1.., s => s.Set(t => t.FirstName, "John"))
                .GenerateList());
        
        Assert.StartsWith(
            "Cannot infer number of elements to create if index-from-end", 
            ex.Message);
    }
    
    [Fact]
    public void Generate_Multiple_GenerateList_NoArg_NoRangeSettersProvided()
    {
        var ex = Assert.Throws<ArgumentException>(() => 
            new Generator<Test>()
                .ForInstance(s => s.Set(t => t.FirstName, "John"))
                .GenerateList());
        
        Assert.Equal(
            "Cannot infer number of elements to create if no range setters are used", 
            ex.Message);
    }
    
    [Fact]
    public void Generate_Multiple_GenerateList_NoArg_IndexFromEndRangeSetterProvided()
    {
        var ex = Assert.Throws<ArgumentException>(() => 
            new Generator<Test>()
                .ForRange(..^2, s => s.Set(t => t.FirstName, "John"))
                .GenerateList());
        
        Assert.StartsWith(
            "Cannot infer number of elements to create if index-from-end", 
            ex.Message);
    }
    
    [Fact]
    public void Generate_Multiple_GenerateList_NoArg_UnboundedMaximumSetterProvided()
    {
        var ex = Assert.Throws<ArgumentException>(() => 
            new Generator<Test>()
                .ForRange(1.., s => s.Set(t => t.FirstName, "John"))
                .GenerateList());
        
        Assert.StartsWith(
            "Cannot infer number of elements to create if index-from-end", 
            ex.Message);
    }
    
    [Fact]
    public void Generate_Multiple_GenerateArray_NoArg_NoRangeSetterProvided()
    {
        var ex = Assert.Throws<ArgumentException>(() => 
            new Generator<Test>()
                .ForInstance(s => s.Set(t => t.FirstName, "John"))
                .GenerateArray());
        
        Assert.Equal(
            "Cannot infer number of elements to create if no range setters are used", 
            ex.Message);
    }

    [Fact]
    public void Generate_ForRange_ForInstanceBefore()
    {
        var items = new Generator<Test>()
            .ForInstance(s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Doe"))
            .ForRange(2.., s => s.Set(t => t.FirstName, "John"))
            .GenerateArray(4);

        Assert.Equal(4, items.Length);
        Assert.All(items[..2], item => Assert.Equal("Jane", item.FirstName));
        Assert.All(items[2..], item => Assert.Equal("John", item.FirstName));
        Assert.All(items, item => Assert.Equal("Doe", item.LastName));
    }

    [Fact]
    public void Generate_ForRange_ForInstanceAfter()
    {
        var items = new Generator<Test>()
            .ForRange(2.., s => s.Set(t => t.FirstName, "John"))
            .ForInstance(s => s
                .Set(t => t.FirstName, "Jane")
                .Set(t => t.LastName, "Doe"))
            .GenerateArray(4);

        Assert.Equal(4, items.Length);
        Assert.All(items[..2], item => Assert.Equal("Jane", item.FirstName));
        Assert.All(items[2..], item => Assert.Equal("John", item.FirstName));
        Assert.All(items, item => Assert.Equal("Doe", item.LastName));
    }

    [Fact]
    public void Generate_ForRange_LastForInstanceOverrides()
    {
        var items = new Generator<Test>()
            .ForInstance(
                s => s
                    .Set(t => t.FirstName, "Jane")
                    .Set(t => t.LastName, "Doe"))
            .ForRange(2..4, s => s.Set(t => t.FirstName, "John"))
            .ForInstance(
                s => s
                    .Set(t => t.FirstName, "Jill")
                    .Set(t => t.LastName, "Roe"))
            .Generate(6)
            .ToArray();

        Assert.Equal(6, items.Length);
        Assert.All(items[..2], item => Assert.Equal("Jill", item.FirstName));
        Assert.All(items[..2], item => Assert.Equal("Roe", item.LastName));
        Assert.All(items[2..4], item => Assert.Equal("John", item.FirstName));
        Assert.All(items[2..4], item => Assert.Equal("Roe", item.LastName));
        Assert.All(items[4..], item => Assert.Equal("Jill", item.FirstName));
        Assert.All(items[4..], item => Assert.Equal("Roe", item.LastName));
    }

    [Fact]
    public void Generate_ForIndex()
    {
        var items = new Generator<Test>()
            .ForInstance(s => s.Set(t => t.FirstName, "Test"))
            .ForInstance(s => s.Set(t => t.LastName, "User"))
            .ForIndex(1, s => s
                .Set(t => t.FirstName, "John")
                .Set(t => t.LastName, "Doe"))
            .GenerateArray(3);

        Assert.Equal(3, items.Length);
        
        Assert.Equal("Test", items[0].FirstName);
        Assert.Equal("User", items[0].LastName);
        
        Assert.Equal("John", items[1].FirstName);
        Assert.Equal("Doe", items[1].LastName);
        
        Assert.Equal("Test", items[2].FirstName);
        Assert.Equal("User", items[2].LastName);
    }

    [Fact]
    public void FinishWith()
    {
        var items = new Generator<Test>()
            .FinishWith(
                t =>
                {
                    t.FirstName = "Joe";
                    t.LastName = "Slow";
                }
            )
            .GenerateList(2);

        Assert.Equal(2, items.Count);
        Assert.All(items, item => Assert.Equal("Joe", item.FirstName));
        Assert.All(items, item => Assert.Equal("Slow", item.LastName));
    }

    [Fact]
    public void FinishWith_Random()
    {
        var items = new Generator<Test>(seeder: () => 10)
            .FinishWith(
                (t, faker) =>
                {
                    t.FirstName = faker.Name.FirstName();
                    t.LastName = "Slow";
                }
            )
            .GenerateList(2);

        Assert.Equal(2, items.Count);
        Assert.All(items, item => Assert.Equal("Valentine", item.FirstName));
        Assert.All(items, item => Assert.Equal("Slow", item.LastName));
    }

    [Fact]
    public void FinishWith_Multiple()
    {
        var items = new Generator<Test>()
            .FinishWith(
                t =>
                {
                    t.FirstName = "Joe";
                    t.LastName = "Slow";
                }
            )
            .FinishWith(
                t =>
                {
                    t.LastName = "Glow";
                }
            )
            .GenerateList(2);

        Assert.Equal(2, items.Count);
        Assert.All(items, item => Assert.Equal("Joe", item.FirstName));
        Assert.All(items, item => Assert.Equal("Glow", item.LastName));
    }

    [Fact]
    public void FinishWith_OverridesOtherSetters()
    {
        var items = new Generator<Test>()
            .ForInstance(
                s => s
                    .Set(t => t.FirstName, "Jane")
                    .Set(t => t.LastName, "Doe"))
            .ForRange(2.., s => s.Set(t => t.FirstName, "John"))
            .FinishWith(
                (t, _) =>
                {
                    t.FirstName = "Joe";
                    t.LastName = "Slow";
                }
            )
            .GenerateList(4);

        Assert.Equal(4, items.Count);
        Assert.All(items, item => Assert.Equal("Joe", item.FirstName));
        Assert.All(items, item => Assert.Equal("Slow", item.LastName));
    }
}
