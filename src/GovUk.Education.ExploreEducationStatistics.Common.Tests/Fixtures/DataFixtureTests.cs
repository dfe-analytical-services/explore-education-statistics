#nullable enable
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

public class DataFixtureTests
{
    private class Person
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class Company
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Generator_Single_RandomIsDeterministic()
    {
        var fixture = new DataFixture();
        var generator = fixture.Generator<Person>();

        var items = generator
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Moises", items[0].FirstName);
        Assert.Equal("Delores", items[1].FirstName);
        Assert.Equal("Ole", items[2].FirstName);

        items = generator.GenerateList(3);

        Assert.Equal("Eldora", items[0].FirstName);
        Assert.Equal("Ray", items[1].FirstName);
        Assert.Equal("Eula", items[2].FirstName);
    }

    [Fact]
    public void Generator_Single_SetSeed()
    {
        var fixture = new DataFixture();
        var generator = fixture.Generator<Person>();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Moises", items[0].FirstName);
        Assert.Equal("Delores", items[1].FirstName);
        Assert.Equal("Ole", items[2].FirstName);

        fixture.SetSeed<Person>(100);

        items = generator
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        // Set the seed manually to some number. It should
        // generate a new set of deterministic random data.
        Assert.Equal("Jennyfer", items[0].FirstName);
        Assert.Equal("Adrain", items[1].FirstName);
        Assert.Equal("Kailee", items[2].FirstName);
    }

    [Fact]
    public void Generator_Single_Index_IncrementsFromZeroOnMultipleGenerates()
    {
        var fixture = new DataFixture();
        var generator = fixture.Generator<Person>();

        var items = generator
            .ForInstance(s => s
                .Set(p => p.FirstName, (faker, _, context) => $"Jane {context.Index}"))
            .GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);

        items = generator.GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);
    }

    [Fact]
    public void Generator_Single_IndexFaker_DoesNotIncrementFromZeroOnMultipleGenerates()
    {
        var fixture = new DataFixture();
        var generator = fixture.Generator<Person>();

        var items = generator
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => $"Jane {faker.IndexFaker}"))
            .GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);

        items = generator.GenerateList(3);

        Assert.Equal("Jane 3", items[0].FirstName);
        Assert.Equal("Jane 4", items[1].FirstName);
        Assert.Equal("Jane 5", items[2].FirstName);
    }

    [Fact]
    public void Generator_Multiple_RandomIsDeterministic()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Moises", items[0].FirstName);
        Assert.Equal("Delores", items[1].FirstName);
        Assert.Equal("Ole", items[2].FirstName);

        items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        // Multiple generator instances should produce
        // new sets of deterministic random data.
        Assert.Equal("Eldora", items[0].FirstName);
        Assert.Equal("Ray", items[1].FirstName);
        Assert.Equal("Eula", items[2].FirstName);
    }

    [Fact]
    public void Generator_Multiple_SetSeed()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Moises", items[0].FirstName);
        Assert.Equal("Delores", items[1].FirstName);
        Assert.Equal("Ole", items[2].FirstName);

        items = fixture
            .SetSeed<Person>(100)
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        // Set the seed manually to some number. It should
        // generate a new set of deterministic random data.
        Assert.Equal("Jennyfer", items[0].FirstName);
        Assert.Equal("Adrain", items[1].FirstName);
        Assert.Equal("Kailee", items[2].FirstName);
    }

    [Fact]
    public void Generator_Multiple_IndexFaker_DoesNotIncrementForGeneratorsOfSameType()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => $"Jane {faker.IndexFaker}")
                .Set(p => p.LastName, faker => $"Doe {faker.IndexFaker}"))
            .GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);

        Assert.Equal("Doe 0", items[0].LastName);
        Assert.Equal("Doe 1", items[1].LastName);
        Assert.Equal("Doe 2", items[2].LastName);

        items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, faker => $"Jane {faker.IndexFaker}")
                .Set(p => p.LastName, faker => $"Doe {faker.IndexFaker}"))
            .GenerateList(3);

        // IndexFaker does not increment when generators
        // of the same type generate new instances.
        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);

        Assert.Equal("Doe 0", items[0].LastName);
        Assert.Equal("Doe 1", items[1].LastName);
        Assert.Equal("Doe 2", items[2].LastName);
    }

    [Fact]
    public void Generator_Multiple_FixtureTypeIndex_IncrementsForGeneratorsOfSameType()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureTypeIndex}")
                .Set(p => p.LastName, (_, _, context) => $"Doe {context.FixtureTypeIndex}"))
            .GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);

        Assert.Equal("Doe 0", items[0].LastName);
        Assert.Equal("Doe 1", items[1].LastName);
        Assert.Equal("Doe 2", items[2].LastName);

        items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureTypeIndex}")
                .Set(p => p.LastName, (_, _, context) => $"Doe {context.FixtureTypeIndex}"))
            .GenerateList(3);

        // FixtureTypeIndex increments when generators
        // of the same type generate new instances.
        Assert.Equal("Jane 3", items[0].FirstName);
        Assert.Equal("Jane 4", items[1].FirstName);
        Assert.Equal("Jane 5", items[2].FirstName);

        Assert.Equal("Doe 3", items[0].LastName);
        Assert.Equal("Doe 4", items[1].LastName);
        Assert.Equal("Doe 5", items[2].LastName);
    }

    [Fact]
    public void Generate_Multiple_FixtureIndex_IncrementsForGeneratorsOfAnyType()
    {
        var fixture = new DataFixture();

        var persons = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureIndex}")
                .Set(p => p.LastName, (_, _, context) => $"Doe {context.FixtureIndex}"))
            .GenerateList(3);

        Assert.Equal("Jane 0", persons[0].FirstName);
        Assert.Equal("Jane 1", persons[1].FirstName);
        Assert.Equal("Jane 2", persons[2].FirstName);

        Assert.Equal("Doe 0", persons[0].LastName);
        Assert.Equal("Doe 1", persons[1].LastName);
        Assert.Equal("Doe 2", persons[2].LastName);

        var companies = fixture
            .Generator<Company>()
            .ForInstance(s => s
                .Set(c => c.Name, (_, _, context) => $"Acme {context.FixtureIndex}"))
            .GenerateList(3);

        // FixtureTypeIndex increments when generators
        // of any type generate new instances.
        Assert.Equal("Acme 3", companies[0].Name);
        Assert.Equal("Acme 4", companies[1].Name);
        Assert.Equal("Acme 5", companies[2].Name);
    }

    [Fact]
    public void Generator_Multiple_SetDefaults_IncrementsForGeneratorsOfSameType()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .SetDefault(p => p.FirstName)
                .SetDefault(p => p.LastName))
            .GenerateList(3);

        Assert.Equal("FirstName of Person 0", items[0].FirstName);
        Assert.Equal("FirstName of Person 1", items[1].FirstName);
        Assert.Equal("FirstName of Person 2", items[2].FirstName);

        Assert.Equal("LastName of Person 0", items[0].LastName);
        Assert.Equal("LastName of Person 1", items[1].LastName);
        Assert.Equal("LastName of Person 2", items[2].LastName);

        items = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .SetDefault(p => p.FirstName)
                .SetDefault(p => p.LastName))
            .GenerateList(3);

        // The default string increments when generators
        // of the same type generate new instances.
        Assert.Equal("FirstName of Person 3", items[0].FirstName);
        Assert.Equal("FirstName of Person 4", items[1].FirstName);
        Assert.Equal("FirstName of Person 5", items[2].FirstName);

        Assert.Equal("LastName of Person 3", items[0].LastName);
        Assert.Equal("LastName of Person 4", items[1].LastName);
        Assert.Equal("LastName of Person 5", items[2].LastName);
    }

    [Fact]
    public void Generator_Multiple_ForRange_SetDefaults_IncrementsForGeneratorsOfSameType()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForRange(..3, s => s
                .SetDefault(p => p.FirstName)
                .SetDefault(p => p.LastName))
            .GenerateList(3);

        Assert.Equal("FirstName of Person 0", items[0].FirstName);
        Assert.Equal("FirstName of Person 1", items[1].FirstName);
        Assert.Equal("FirstName of Person 2", items[2].FirstName);

        Assert.Equal("LastName of Person 0", items[0].LastName);
        Assert.Equal("LastName of Person 1", items[1].LastName);
        Assert.Equal("LastName of Person 2", items[2].LastName);

        items = fixture
            .Generator<Person>()
            .ForRange(..3, s => s
                .SetDefault(p => p.FirstName)
                .SetDefault(p => p.LastName))
            .GenerateList(3);

        // The default string increments when generators
        // of the same type generate new instances.
        Assert.Equal("FirstName of Person 3", items[0].FirstName);
        Assert.Equal("FirstName of Person 4", items[1].FirstName);
        Assert.Equal("FirstName of Person 5", items[2].FirstName);

        Assert.Equal("LastName of Person 3", items[0].LastName);
        Assert.Equal("LastName of Person 4", items[1].LastName);
        Assert.Equal("LastName of Person 5", items[2].LastName);
    }

    [Fact]
    public void Generate_Multiple_SetDefaults_DoesNotIncrementForGeneratorsOfOtherTypes()
    {
        var fixture = new DataFixture();

        var persons = fixture
            .Generator<Person>()
            .ForInstance(s => s
                .SetDefault(p => p.FirstName)
                .SetDefault(p => p.LastName))
            .GenerateList(3);

        Assert.Equal("FirstName of Person 0", persons[0].FirstName);
        Assert.Equal("FirstName of Person 1", persons[1].FirstName);
        Assert.Equal("FirstName of Person 2", persons[2].FirstName);

        Assert.Equal("LastName of Person 0", persons[0].LastName);
        Assert.Equal("LastName of Person 1", persons[1].LastName);
        Assert.Equal("LastName of Person 2", persons[2].LastName);

        var companies = fixture
            .Generator<Company>()
            .ForInstance(s => s
                .SetDefault(c => c.Name))
            .GenerateList(3);

        // The default string does not increment when
        // generators of other types generate new instances.
        Assert.Equal("Name of Company 0", companies[0].Name);
        Assert.Equal("Name of Company 1", companies[1].Name);
        Assert.Equal("Name of Company 2", companies[2].Name);
    }


    [Fact]
    public void Generate_Multiple_ForRange_SetDefaults_DoesNotIncrementForGeneratorsOfOtherTypes()
    {
        var fixture = new DataFixture();

        var persons = fixture
            .Generator<Person>()
            .ForRange(..3, s => s
                .SetDefault(p => p.FirstName)
                .SetDefault(p => p.LastName))
            .GenerateList(3);

        Assert.Equal("FirstName of Person 0", persons[0].FirstName);
        Assert.Equal("FirstName of Person 1", persons[1].FirstName);
        Assert.Equal("FirstName of Person 2", persons[2].FirstName);

        Assert.Equal("LastName of Person 0", persons[0].LastName);
        Assert.Equal("LastName of Person 1", persons[1].LastName);
        Assert.Equal("LastName of Person 2", persons[2].LastName);

        var companies = fixture
            .Generator<Company>()
            .ForRange(..3, s => s
                .SetDefault(c => c.Name))
            .GenerateList(3);

        // The default string does not increment when
        // generators of other types generate new instances.
        Assert.Equal("Name of Company 0", companies[0].Name);
        Assert.Equal("Name of Company 1", companies[1].Name);
        Assert.Equal("Name of Company 2", companies[2].Name);
    }
}
