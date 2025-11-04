#nullable enable
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

public class DataFixtureTests
{
    // ReSharper disable once ClassNeverInstantiated.Local
    private class Person
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
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
            .ForInstance(s => s.Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Lenore", items[0].FirstName);
        Assert.Equal("Breanne", items[1].FirstName);
        Assert.Equal("Macy", items[2].FirstName);

        items = generator.GenerateList(3);

        Assert.Equal("Chanelle", items[0].FirstName);
        Assert.Equal("Maxie", items[1].FirstName);
        Assert.Equal("Dakota", items[2].FirstName);
    }

    [Fact]
    public void Generator_Single_GenerateRandomIsDeterministic()
    {
        var fixture = new DataFixture();
        var generator = fixture.Generator<Person>();

        var items = generator.ForInstance(s => s.Set(p => p.FirstName, "Test person")).GenerateRandomList(10);

        Assert.Equal(7, items.Count);
        Assert.All(items, item => Assert.Equal("Test person", item.FirstName));

        items = generator.GenerateRandomList(10);

        Assert.Equal(8, items.Count);
        Assert.All(items, item => Assert.Equal("Test person", item.FirstName));
    }

    [Fact]
    public void Generator_Single_SetSeed()
    {
        var fixture = new DataFixture();
        var generator = fixture.Generator<Person>();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s.Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Lenore", items[0].FirstName);
        Assert.Equal("Breanne", items[1].FirstName);
        Assert.Equal("Macy", items[2].FirstName);

        fixture.SetSeed<Person>(100);

        items = generator.ForInstance(s => s.Set(p => p.FirstName, faker => faker.Name.FirstName())).GenerateList(3);

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
            .ForInstance(s => s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.Index}"))
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
    public void Generator_Single_DefaultDataFixture_Index_IncrementsFromZeroOnMultipleGenerates()
    {
        var generator = new Generator<Person>();

        var items = generator
            .ForInstance(s => s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.Index}"))
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
    public void Generator_Single_DefaultDataFixture_FixtureIndex_IncrementSharedAcrossMultipleGenerates()
    {
        var generator = new Generator<Person>();

        var items = generator
            .ForInstance(s => s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureIndex}"))
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
    public void Generator_Single_BuiltInDataFixture_FixtureTypeIndex_IncrementSharedAcrossMultipleGenerates()
    {
        var generator = new Generator<Person>();

        var items = generator
            .ForInstance(s => s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureTypeIndex}"))
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
    public void Generator_Single_IndexFaker_DoesNotIncrementFromZeroOnMultipleGenerates()
    {
        var fixture = new DataFixture();
        var generator = fixture.Generator<Person>();

        var items = generator
            .ForInstance(s => s.Set(p => p.FirstName, faker => $"Jane {faker.IndexFaker}"))
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
            .ForInstance(s => s.Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Lenore", items[0].FirstName);
        Assert.Equal("Breanne", items[1].FirstName);
        Assert.Equal("Macy", items[2].FirstName);

        items = fixture
            .Generator<Person>()
            .ForInstance(s => s.Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        // Multiple generator instances should produce
        // new sets of deterministic random data.
        Assert.Equal("Chanelle", items[0].FirstName);
        Assert.Equal("Maxie", items[1].FirstName);
        Assert.Equal("Dakota", items[2].FirstName);
    }

    [Fact]
    public void Generator_Multiple_GenerateRandomIsDeterministic()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s.Set(p => p.FirstName, "Test person 1"))
            .GenerateRandomList(10);

        Assert.Equal(7, items.Count);
        Assert.All(items, item => Assert.Equal("Test person 1", item.FirstName));

        items = fixture
            .Generator<Person>()
            .ForInstance(s => s.Set(p => p.FirstName, "Test person 2"))
            .GenerateRandomList(10);

        Assert.Equal(8, items.Count);
        Assert.All(items, item => Assert.Equal("Test person 2", item.FirstName));
    }

    [Fact]
    public void Generator_Multiple_RandomIsDeterministicButDifferentPerType()
    {
        var fixture = new DataFixture();

        var peopleGenerator = fixture.Generator<Person>();

        var people = peopleGenerator
            .ForInstance(s => s.Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Lenore", people[0].FirstName);
        Assert.Equal("Breanne", people[1].FirstName);
        Assert.Equal("Macy", people[2].FirstName);

        var companyGenerator = fixture.Generator<Company>();

        var companies = companyGenerator
            .ForInstance(s => s.Set(p => p.Name, faker => faker.Name.FirstName()))
            .GenerateList(3);

        // The companyGenerator's faker instance is providing a different set of deterministic names
        // than the personGenerator, because they are generators of different types. This helps us to
        // avoid generating similar data for different types so that, for instance, we can avoid
        // generating clashing information like ids between different data types.
        Assert.Equal("Marcia", companies[0].Name);
        Assert.Equal("Clay", companies[1].Name);
        Assert.Equal("Michale", companies[2].Name);
    }

    [Fact]
    public void Generator_Multiple_SetSeed()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s.Set(p => p.FirstName, faker => faker.Name.FirstName()))
            .GenerateList(3);

        Assert.Equal("Lenore", items[0].FirstName);
        Assert.Equal("Breanne", items[1].FirstName);
        Assert.Equal("Macy", items[2].FirstName);

        items = fixture
            .SetSeed<Person>(100)
            .Generator<Person>()
            .ForInstance(s => s.Set(p => p.FirstName, faker => faker.Name.FirstName()))
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
            .ForInstance(s =>
                s.Set(p => p.FirstName, faker => $"Jane {faker.IndexFaker}")
                    .Set(p => p.LastName, faker => $"Doe {faker.IndexFaker}")
            )
            .GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);

        Assert.Equal("Doe 0", items[0].LastName);
        Assert.Equal("Doe 1", items[1].LastName);
        Assert.Equal("Doe 2", items[2].LastName);

        items = fixture
            .Generator<Person>()
            .ForInstance(s =>
                s.Set(p => p.FirstName, faker => $"Jane {faker.IndexFaker}")
                    .Set(p => p.LastName, faker => $"Doe {faker.IndexFaker}")
            )
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
            .ForInstance(s =>
                s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureTypeIndex}")
                    .Set(p => p.LastName, (_, _, context) => $"Doe {context.FixtureTypeIndex}")
            )
            .GenerateList(3);

        Assert.Equal("Jane 0", items[0].FirstName);
        Assert.Equal("Jane 1", items[1].FirstName);
        Assert.Equal("Jane 2", items[2].FirstName);

        Assert.Equal("Doe 0", items[0].LastName);
        Assert.Equal("Doe 1", items[1].LastName);
        Assert.Equal("Doe 2", items[2].LastName);

        items = fixture
            .Generator<Person>()
            .ForInstance(s =>
                s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureTypeIndex}")
                    .Set(p => p.LastName, (_, _, context) => $"Doe {context.FixtureTypeIndex}")
            )
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
    public void Generate_Multiple_DefaultDataFixtures_FixtureTypeIndex_IncrementIndependently()
    {
        // Instantiate generators without a shared `DataFixture` instance,
        // meaning each generator gets its own default instance.
        var persons = new Generator<Person>()
            .ForInstance(s =>
                s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureTypeIndex}")
                    .Set(p => p.LastName, (_, _, context) => $"Doe {context.FixtureTypeIndex}")
            )
            .GenerateList(3);

        Assert.Equal("Jane 0", persons[0].FirstName);
        Assert.Equal("Jane 1", persons[1].FirstName);
        Assert.Equal("Jane 2", persons[2].FirstName);

        Assert.Equal("Doe 0", persons[0].LastName);
        Assert.Equal("Doe 1", persons[1].LastName);
        Assert.Equal("Doe 2", persons[2].LastName);

        var companies = new Generator<Company>()
            .ForInstance(s => s.Set(c => c.Name, (_, _, context) => $"Acme {context.FixtureTypeIndex}"))
            .GenerateList(3);

        Assert.Equal("Acme 0", companies[0].Name);
        Assert.Equal("Acme 1", companies[1].Name);
        Assert.Equal("Acme 2", companies[2].Name);
    }

    [Fact]
    public void Generate_Multiple_FixtureIndex_IncrementsForGeneratorsOfAnyType()
    {
        var fixture = new DataFixture();

        var persons = fixture
            .Generator<Person>()
            .ForInstance(s =>
                s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureIndex}")
                    .Set(p => p.LastName, (_, _, context) => $"Doe {context.FixtureIndex}")
            )
            .GenerateList(3);

        Assert.Equal("Jane 0", persons[0].FirstName);
        Assert.Equal("Jane 1", persons[1].FirstName);
        Assert.Equal("Jane 2", persons[2].FirstName);

        Assert.Equal("Doe 0", persons[0].LastName);
        Assert.Equal("Doe 1", persons[1].LastName);
        Assert.Equal("Doe 2", persons[2].LastName);

        var companies = fixture
            .Generator<Company>()
            .ForInstance(s => s.Set(c => c.Name, (_, _, context) => $"Acme {context.FixtureIndex}"))
            .GenerateList(3);

        // FixtureIndex increments when generators
        // of any type generate new instances.
        Assert.Equal("Acme 3", companies[0].Name);
        Assert.Equal("Acme 4", companies[1].Name);
        Assert.Equal("Acme 5", companies[2].Name);
    }

    [Fact]
    public void Generate_Multiple_DefaultDataFixtures_FixtureIndex_IncrementIndependently()
    {
        // Instantiate generators without a shared `DataFixture` instance,
        // meaning each generator gets its own default instance.
        var persons = new Generator<Person>()
            .ForInstance(s =>
                s.Set(p => p.FirstName, (_, _, context) => $"Jane {context.FixtureIndex}")
                    .Set(p => p.LastName, (_, _, context) => $"Doe {context.FixtureIndex}")
            )
            .GenerateList(3);

        Assert.Equal("Jane 0", persons[0].FirstName);
        Assert.Equal("Jane 1", persons[1].FirstName);
        Assert.Equal("Jane 2", persons[2].FirstName);

        Assert.Equal("Doe 0", persons[0].LastName);
        Assert.Equal("Doe 1", persons[1].LastName);
        Assert.Equal("Doe 2", persons[2].LastName);

        var companies = new Generator<Company>()
            .ForInstance(s => s.Set(c => c.Name, (_, _, context) => $"Acme {context.FixtureIndex}"))
            .GenerateList(3);

        // FixtureIndex increments when generators
        // of any type generate new instances.
        Assert.Equal("Acme 0", companies[0].Name);
        Assert.Equal("Acme 1", companies[1].Name);
        Assert.Equal("Acme 2", companies[2].Name);
    }

    [Fact]
    public void Generator_Multiple_SetDefaults_IncrementsForGeneratorsOfSameType()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForInstance(s => s.SetDefault(p => p.FirstName).SetDefault(p => p.LastName))
            .GenerateList(3);

        Assert.Equal("Person 0 :: FirstName", items[0].FirstName);
        Assert.Equal("Person 1 :: FirstName", items[1].FirstName);
        Assert.Equal("Person 2 :: FirstName", items[2].FirstName);

        Assert.Equal("Person 0 :: LastName", items[0].LastName);
        Assert.Equal("Person 1 :: LastName", items[1].LastName);
        Assert.Equal("Person 2 :: LastName", items[2].LastName);

        items = fixture
            .Generator<Person>()
            .ForInstance(s => s.SetDefault(p => p.FirstName).SetDefault(p => p.LastName))
            .GenerateList(3);

        // The default string increments when generators
        // of the same type generate new instances.
        Assert.Equal("Person 3 :: FirstName", items[0].FirstName);
        Assert.Equal("Person 4 :: FirstName", items[1].FirstName);
        Assert.Equal("Person 5 :: FirstName", items[2].FirstName);

        Assert.Equal("Person 3 :: LastName", items[0].LastName);
        Assert.Equal("Person 4 :: LastName", items[1].LastName);
        Assert.Equal("Person 5 :: LastName", items[2].LastName);
    }

    [Fact]
    public void Generator_Multiple_ForRange_SetDefaults_IncrementsForGeneratorsOfSameType()
    {
        var fixture = new DataFixture();

        var items = fixture
            .Generator<Person>()
            .ForRange(..3, s => s.SetDefault(p => p.FirstName).SetDefault(p => p.LastName))
            .GenerateList(3);

        Assert.Equal("Person 0 :: FirstName", items[0].FirstName);
        Assert.Equal("Person 1 :: FirstName", items[1].FirstName);
        Assert.Equal("Person 2 :: FirstName", items[2].FirstName);

        Assert.Equal("Person 0 :: LastName", items[0].LastName);
        Assert.Equal("Person 1 :: LastName", items[1].LastName);
        Assert.Equal("Person 2 :: LastName", items[2].LastName);

        items = fixture
            .Generator<Person>()
            .ForRange(..3, s => s.SetDefault(p => p.FirstName).SetDefault(p => p.LastName))
            .GenerateList(3);

        // The default string increments when generators
        // of the same type generate new instances.
        Assert.Equal("Person 3 :: FirstName", items[0].FirstName);
        Assert.Equal("Person 4 :: FirstName", items[1].FirstName);
        Assert.Equal("Person 5 :: FirstName", items[2].FirstName);

        Assert.Equal("Person 3 :: LastName", items[0].LastName);
        Assert.Equal("Person 4 :: LastName", items[1].LastName);
        Assert.Equal("Person 5 :: LastName", items[2].LastName);
    }

    [Fact]
    public void Generate_Multiple_SetDefaults_DoesNotIncrementForGeneratorsOfOtherTypes()
    {
        var fixture = new DataFixture();

        var persons = fixture
            .Generator<Person>()
            .ForInstance(s => s.SetDefault(p => p.FirstName).SetDefault(p => p.LastName))
            .GenerateList(3);

        Assert.Equal("Person 0 :: FirstName", persons[0].FirstName);
        Assert.Equal("Person 1 :: FirstName", persons[1].FirstName);
        Assert.Equal("Person 2 :: FirstName", persons[2].FirstName);

        Assert.Equal("Person 0 :: LastName", persons[0].LastName);
        Assert.Equal("Person 1 :: LastName", persons[1].LastName);
        Assert.Equal("Person 2 :: LastName", persons[2].LastName);

        var companies = fixture.Generator<Company>().ForInstance(s => s.SetDefault(c => c.Name)).GenerateList(3);

        // The default string does not increment when
        // generators of other types generate new instances.
        Assert.Equal("Company 0 :: Name", companies[0].Name);
        Assert.Equal("Company 1 :: Name", companies[1].Name);
        Assert.Equal("Company 2 :: Name", companies[2].Name);
    }

    [Fact]
    public void Generate_Multiple_ForRange_SetDefaults_DoesNotIncrementForGeneratorsOfOtherTypes()
    {
        var fixture = new DataFixture();

        var persons = fixture
            .Generator<Person>()
            .ForRange(..3, s => s.SetDefault(p => p.FirstName).SetDefault(p => p.LastName))
            .GenerateList(3);

        Assert.Equal("Person 0 :: FirstName", persons[0].FirstName);
        Assert.Equal("Person 1 :: FirstName", persons[1].FirstName);
        Assert.Equal("Person 2 :: FirstName", persons[2].FirstName);

        Assert.Equal("Person 0 :: LastName", persons[0].LastName);
        Assert.Equal("Person 1 :: LastName", persons[1].LastName);
        Assert.Equal("Person 2 :: LastName", persons[2].LastName);

        var companies = fixture.Generator<Company>().ForRange(..3, s => s.SetDefault(c => c.Name)).GenerateList(3);

        // The default string does not increment when
        // generators of other types generate new instances.
        Assert.Equal("Company 0 :: Name", companies[0].Name);
        Assert.Equal("Company 1 :: Name", companies[1].Name);
        Assert.Equal("Company 2 :: Name", companies[2].Name);
    }
}
