# Mock Builders
## What are mock builders?
Mock Builders are a simple and elegant pattern for encapsulating the setup and assertion of mock dependencies - be those services or models.

Benefits include:
* encapsulate verbose and often hard to understand setup statements
* provide easy to read arrangement statements
* produce a happy path dependency by default
* allow easy reusability when testing different components that consume the same dependency
* capture and simulate actual behaviour of the real component

## Guiding Principles
Mock Builders help to simplify unit tests and to meet as many of the following guiding principles as is practical. Before we look at how to code a Mock Builder, it may be useful to see how they are consumed.

### Arrange statements should be easily readable
Mocking frameworks such as Moq are powerful but the setup statements can easily become complex. They take time to read and understand.

```csharp
// Powerful, yes. Quick to understand, not so much.
var mock = new Mock<IMyService>(MockBehavior.Strict);
mock.Setup(m => m.DoSomething(It.IsAny<string>(), It.IsAny<CancellationToken>())
.ReturnAsync((id,_)=>new Response{ Id = id, Result = Stage.Started });
mock.Setup(m => m.DoSomething(It.Is<string>(id => id == "ABC"), It.IsAny<CancellationToken>())
.ReturnAsync((id,_)=>new Response{ Id = id, Result = Stage.Failed });
```

By creating a mock builder, this code would be encapsulated inside the builder (more on that later) with easy to read arrangement statements. So the consuming code could look like this:

```csharp
var myService = new MyServiceBuilder()
            .WhereDoSomethingWillFailForId("ABC")
            .Build();
```

They can equally be used to generate models. 

For example, if we needed an instance of an `Address` model but the only field that we were interested in for a particular test was the postcode then we could create a builder to consume as follows:

```csharp
// Instead of having to construct a whole model
var address = new Address
{
    AddressLine1 = "123 Testing Ave",
    AddressLine2 = "Mockingbird Crescent",
    Town = "Sheffield",
    County = "Yorkshire",
    Postcode = "" // missing postcode
};

// we would instead have this:

// A way to set the postcode without setting all of the irrelevant fields
var address1 = new AddressBuilder()
                    .WherePostcodeIs("")
                    .Build();

// Or even nicer
var address2 = new AddressBuilder()
                    .WherePostcodeIsBlank()
                    .Build();
```


### Arrange the bare minimum

> [!TIP]
> Arrange statements should only setup data specific to the condition being asserted

For example, consider a validator for an `Address` model and we want to test for an invalid postcode, there should be no need to arrange anything other than the postcode. In other words, a mock builder would set address lines, town, country etc to something sensible.

So instead of this:
```csharp
var address = new AddressBuilder()
                    .WherePostcodeIsBlank()
                    .Build();

var actual = AddressValidator.Validate(address);
```

This also allows business logic to be encapsulated. For example, what constitutes a populated but invalid postcode?

```csharp
// Consider a code that contains a checksum digit at the end. Instead of having to know how it is calculated, this logic is built into the builder.

// For a test that explicitly tests for a valid checksum, it can be useful to call the builder with an explicit statement that the checksum is valid. However, for other tests, it can be omitted since a valid code would be the default happy path
var goodCode = new CodeBuilder().WithValidChecksum().Build();
var badCode = new CodeBuilder().WithInvalidChecksum().Build();

```

## How to write a Mock Builder

Let's go step by step to creating a Mock Builder for a fictional service called `IReferenceService`. 

Let's define it as:

```csharp
public interface IReferenceService
{
    Task<string?> GetReferenceCodeForPerson(Person person)
}

public record Person(Guid id, string FirstName, string LastName);
```

### The Basics

* create a mock for our component - one instance per builder instance
* create the `Build` method for obtaining the constructed component

```csharp
// Best to name the class [Type name]Builder
public class ReferenceServiceBuilder
{
    // Create the mock object using your favoured mocking framework.
    // We'll use Moq here.
    // *Top Tip* - always use Strict mode.
    private readonly Mock<IReferenceService> _mock = new Mock<IReferenceService>(MockBehavior.Strict);

    // The Build method which returns the mock
    public IReferenceService Build()
    {
        // Setups - we'll cover this shortly

        return _mock.Object;
    }
}

```
> [!TIP]
> Always use Strict mode on your mocks

The argument for using Strict mode is that you (almost) never want any call being made to your service returning a `default` result and the code execution carrying on. In practice, this leads to a failure later on that then needs to be troubleshooted and tracing back to the missing setup call. Trust me - you'll thank me later.

### Arrangements / Setups

* Make the setup method names as easy to understand as possible
* Add more methods if it makes it clearer
* Return `this` to provide a fluent interface

```csharp
public class ReferenceServiceBuilder
{
    private readonly Mock<IReferenceService> _mock = new Mock<IReferenceService>(MockBehavior.Strict);

    public IReferenceService Build()
    {
        // We'll add some more stuff here shortly...
        return _mock.Object;
    }
```
Simple first case. For a given call to retrieve a person, lets return a test code.
Return `this` to provide a fluent interface
```csharp
    // Define that for a specified person, return this code.
    // Attempting to get a code for anyone else will fail due to the mock being strict!
    public ReferenceServiceBuilder WhereCodeForPersonIs(Person person, string code)
    {
        _mock
            .Setup(m => m.GetReferenceCodeForPerson(person))
            .ReturnsAsync(code);
        return this;
    }
```
What if you don't care about which Person is being passed in? No problem.
```csharp

    // Just return this code regardless
    public ReferenceServiceBuilder WhereCodeReturnedIs(string code)
    {
        _mock
            .Setup(m => m.GetReferenceCodeForPerson(It.IsAny<Person>()))
            .ReturnsAsync(code);
        return this;
    }
}
```

What about if the person is not found?

This is a common scenario when making a call (especially to a third party service) when you might not know how the real service behaves. Does it return null? Does it throw an exception?

```csharp
    // Simulate when the Person is not found
    public ReferenceServiceBuilder WherePersonNotFound(Person person)
    {
        _mock
            .Setup(m => m.GetReferenceCodeForPerson(It.IsAny<Person>()))
            .ReturnsAsync((Person?)null);
        return this;
    }
```

#### Happy Path by Default

We want the happy path to be the default though. In the above example, if we do not call at least one of the setups then any call to the service will result in a mock exception. Therefore, it is useful to set the default state of the Mock Builder to return something useful.

It is important though that if you want to assert on something, then that should be part of the setup. Do not just assert on what you believe to be the default happy state since that should be free to change.

```csharp
public class ReferenceServiceBuilder
{
    private readonly Mock<IReferenceService> _mock = new Mock<IReferenceService>(MockBehavior.Strict);

    public ReferenceServiceBuilder
    {
        // Utilise our easy to read methods.
        // Also, by calling these first, we can override them later
        WhereCodeReturnedIs("Default Test Code");
    }

    public IReferenceService Build()
    {
        return _mock.Object;
    }

    public ReferenceServiceBuilder WhereCodeReturnedIs(string code)
    {
        _mock
            .Setup(m => m.GetReferenceCodeForPerson(It.IsAny<Person>()))
            .ReturnsAsync(code);
        return this;
    }
```


### Assertions

In a similar way to setups, we want to encapsulate the assertions.

Let's make sure our service was called for a specified Person

```csharp
    // Ensure that a person was queried for
    public void AssertThatReferenceCodeWasRequestedFor(Person person)
    {
        _mock.Verify(m => m.GetReferenceCodeForPerson(person), Times.Once);
    }
```

Or ensure the service was called N times

```csharp
    // Ensure that a person was queried for
    public void AssertThatReferenceCodeWasRequestedNTimes(int expectedNumberOfQueries)
    {
        _mock.Verify(m => m.GetReferenceCodeForPerson(It.IsAny<Person>()), Times.Exactly(expectedNumberOfQueries));
    }
```

### Assertions++

A nice way to separate the assertions is to use a subclass called `Asserter` which is exposed through a property called `Assert`. This not only moves the assertions into a separate area, but it also slightly clarifies the intention of the consumer.

The basic setup is as follows:

```csharp
public class ReferenceServiceBuilder
{
    private Mock<IReferenceService> _mock = new Mock<IReferenceService>(MockBehavior.Strict);

    public IReferenceService Build()
    {
        return _mock.Object;
    }

    // Expose the Asserter through a property.
    // Yes - it is slightly more efficient if there are lots of calls to this property to store a single instance in a field and return that if you want. A few excessive allocations in the unit tests aren't catastrophic.
    public Asserter Assert => new (_mock);

    public class Asserter(Mock<IReferenceService> mock)
    {
        // Assertion statements go here instead of in the Builder.
        // No need to prefix the methods with Assert now
        public void ReferenceCodeWasRequestedNTimes(int expectedNumberOfQueries)
        {
            _mock.Verify(m => m.GetReferenceCodeForPerson(It.IsAny<Person>()), Times.Exactly(expectedNumberOfQueries));
        }
    }
    ...
}

// Usage:
referenceServiceBuilder.Assert.ReferenceCodeWasRequestedNTimes(3);

```


### Advanced Setups

#### Nested Builders

Quite often, setting up a mock can involve other mocks. For example, a call to a service that returns a complex object which itself requires some setting up.

Let's consider an address service that returns an address.

```csharp
public interface IAddressService
{
    Address GetHomeAddressForPerson(Person person);
}

public record Person(Guid id, string FirstName, string LastName);

public record Address
{
    required string AddressLine1 { get;init; }
    required string AddressLine2  { get;init; }
    required string Town  { get;init; }
    required string County { get;init; }
    required string Postcode { get;init; }
};
```

In this scenario, we could setup a Mock Builder as follows.

Let's setup the happy path first.

```csharp

public class AddressServiceBuilder
{
    private readonly Mock<IAddressService> _mock = new Mock<IAddressService>(MockBehavior.Strict);

    // We can hang on to a default address if one is provided
    private Address? _address;

    public IAddressService Build()
    {
        // By having the setup here, we know that it is always setup.
        // The default case is a happy path.
        _mock
            .Setup(m => m.GetHomeAddressForPerson(It.IsAny<Person>()))
            .Returns(_address ?? new Address{ ... }); // Create a default address
        return _mock.Object;
    }

    public AddressServiceBuilder WhereAddressReturnedIs(Address address)
    {
        _address = address;
        return this;
    }
}

// Usage: No additional setup made
var addressService = new AddressServiceBuilder().Build();
// This will return the default address
var actual = addressService.GetHomeAddressForPerson(person);


```
You could ask whether we could simply define the default Address instance in the field declaration. And yes, you could do that and then overwrite it. It might be useful to be able to differentiate between the default value and a user supplied one.

``` csharp
// Usage: Specify an address
var address = new Address
{
    AddressLine1 = "123 Testing Ave",
    AddressLine2 = "Mockingbird Crescent",
    Town = "Sheffield",
    County = "Yorkshire",
    Postcode = "AB1 2CD"
};
var addressService = new AddressServiceBuilder()
                        .WhereAddressReturnedIs(address)
                        .Build();
// This will now return the specified address
var actual = addressService.GetHomeAddressForPerson(person);
```

Alternatively, instead of having to declare an entire `Address` instance, we could use an `AddressBuilder` that we'd also created earlier.


``` csharp
// Usage: Use an address mock builder to only set the information we care about
var address = new AddressBuilder()
.WhereThePostcodeIs("FG1 2HJ")
.Build();

var addressService = new AddressServiceBuilder()
                        .WhereAddressReturnedIs(address)
                        .Build();

// This will now return the specified address
var actual = addressService.GetHomeAddressForPerson(person);
```

We could be even more fancy and place the Address Builder inside the Address Service and expose it to allow this sort of thing.

```csharp

public class AddressServiceBuilder
{
    private readonly Mock<IAddressService> _mock = new Mock<IAddressService>(MockBehavior.Strict);

    // Use our own address builder to create the address
    private AddressBuilder _addressBuilder = new();

    public IAddressService Build()
    {
         _mock
            .Setup(m => m.GetHomeAddressForPerson(It.IsAny<Person>()))
            .Returns(_addressBuilder.Build()); // Resolve the address from the address builder
        return _mock.Object;
    }

    public AddressServiceBuilder WhereAddressReturnedIs(Action<AddressBuilder> modifyAddress)
    {
        modifyAddress(_addressBuilder);
        return this;
    }
}

// Usage
var addressService = new AddressServiceBuilder()
                        .WhereAddressReturnedIs(a => a.WherePostcodeIsBlank())
                        .Build();

// This will now return an address with a missing postcode
var actual = addressService.GetHomeAddressForPerson(person);
```

# Usage in Unit Tests - More tips!

Here are even more tips for using Mock Builders in your unit tests.

## Use a factory method e.g. GetSUT()
> [!TIP]
> Use a method to instantiate your System Under Test (SUT)

There are several options for creating the SUT
* instantiate the SUT in the constructor and assign to a field
* instantiate the SUT in every unit test

Some reasons to have a single method for instantiating the subject of your test are:
* provides a single place to update when the constructor changes
* it allows preconditions to be set before the SUT is instantiated e.g. config settings

## Declare the dependencies of your SUT as readonly fields

Let's consider writing a unit test for this method.
```csharp

// My Weather Emailer obtains the weather forecast and sends an email
public class WeatherEmailer(IWeatherProvider weatherProvider, IEmailSender emailSender) : IWeatherEmailer
{
    public Task SendWeatherEmail()
    {
        var weatherForecast = await weatherProvider.GetWeatherForecast();
        var email = ConstructWeatherEmail(weatherForecast);
        await emailSender.SendEmail(email);
    }
}

```
Here is a typical setup for a unit test for the above WeatherEmailer
```csharp
public class WeatherEmailerTests
{
    // Set the builders a fields to allow the test to manipulate them.
    // Make them readonly. No need to be reassigning them.
    private readonly WeatherProviderBuilder _weatherProviderBuilder = new();
    private readonly EmailSenderBuilder _emailSenderBuilder = new();

    private IWeatherEmailer GetSut() => 
        new WeatherEmailer(_weatherProviderBuilder.Build(), _emailSenderBuilder.Build());
    
    // Extremely useful to ensure the SUT can be instantiated successfully.
    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());
}
```

## Arrange, Act, Assert
Methods should be simple things - there is a precondition, the execution, and then a postcondition.
If a method can not be defined like this then it is often indicative that it is doing too much and can be split up.

> [!TIP]
> Define these three distinct sections of a test: Arrange, Act, Assert.

Arranging a test into Arrange, Act and Assert sections within a test helps devlopers to identify 
* Arrange: what is being setup for the execution
* Act: what execution is being tested in this test
* Assert: the expectations after the execution

> [!TIP]
> Have your Act be a single statement, where possible.

In most cases, it should be possible to perform the Act in a single statement. If there is a call to your SUT that it required before the execution being tested, then that is part of the Arrangement. And that other call will also have its own test - so it can be assumed to be working subsequently.

> [!TIP]
> Catch exceptions in Act, test them in Assert

```csharp

// Only catch the expected exception in ACT
// ACT
var exception = Record.Exception(() => sut.DoSomething());
// Async version
var exception = await Record.ExceptionAsync(() => sut.SendWeatherEmail);

// Then assert that the exception is the expected type and contains the expected information
// ASSERT
var weatherException = Assert.OfType<WeatherException>(exception);
Assert.True(weatherException.IsTooCold)
```
