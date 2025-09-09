# Explore Education Statistics - Backend Style Guide

<details markdown="1">
  <summary>Table of Contents</summary>

-   [1 Background](#s1)
-   [2 C# Language Rules](#s2)
    *   [2.1 Style](#s2.1)
        +   [2.1.1 File Scoped Namespaces](#s2.1.1)
        +   [2.1.2 Implicitly Typed Local Variables](#s2.1.2)
        +   [2.1.3 Named Parameters](#s2.1.3)
        +   [2.1.4 Nullable Reference Types](#s2.1.4)
    *   [2.2 Structure](#s2.2)
        +   [2.2.1 Entity Framework (EF)](#s2.2.1)
            +   [2.2.1.1 Annotations VS Fluent API](#s2.2.1.1)
            +   [2.2.1.2 Stored Procedures VS Linq VS SQL](#s2.2.1.2)
    *   [2.3 Best Practices](#s2.3)
        +   [2.3.1 Collections](#s2.3.1)
        +   [2.3.2 Entity Framework (EF)](#s2.3.2)
-   [3 C# Testing Rules](#s3)
    *   [3.1 Style](#s3.1)
        +   [3.1.1 Test Naming Convention](#s3.1.1)
        +   [3.1.2 Separating common aspects of tests from the main test suites](#s3.1.2)
    *   [3.2 Test Setup](#s3.2)
        +   [3.2.1 Test Data Generators](#s3.2.1)
        +   [3.2.2 Mocking](#s3.2.2)
            +   [3.2.2.1 Creating Mocks](#s3.2.2.1)
            +   [3.2.2.2 When To Use Mocks](#s3.2.2.2)
    *   [3.3 Best Practices](#s3.3)
        +   [3.3.1 Synchronous VS Asynchronous Test Methods](#s3.3.1)
        +   [3.3.2 Integration VS Unit Tests](#s3.3.2)

</details>

<a id="s1"></a>
## 1 Background 

C# is the backend language used in this repository. This style guide is a list
of agreed coding standards to conform to when writing C# code for this repository.

<a id="s2"></a>
## 2 C# Language Rules 

<a id="s2.1"></a>
### 2.1 Style 

<a id="s2.1.1"></a>
#### 2.1.1 File Scoped Namespaces

- Use file scoped namespace declarations e.g `namespace X.Y.Z;` over standard namespace declarations e.g. `namespace X.Y.Z { ... }` when creating new files.
- Change existing files to use file scoped namespace declarations only if there are significant line changes or if files are small, to minimise the impact on readability of pull requests.

<a id="s2.1.2"></a>
#### 2.1.2 Implicitly Typed Local Variables and Constants

- Prefer using the `var` keyword to declare local variables without specifying an explicit type when the type is obvious from the right-hand side.
- Use the `const` keyword to declare local numbers, Boolean values, strings, or null values as compile-time constants if they are not expected to change.

<a id="s2.1.3"></a>
#### 2.1.3 Named Parameters

- As a rule, if we hit 3 or more parameters for a method call, consider using named parameters, or break out a class/record to hold the parameters*
- If we have multiple parameters of the same type, use named parameters to protect against the accidental reordering of parameters*

*This includes constructors

<a id="s2.1.4"></a>
#### 2.1.4 Nullable Reference Types

- Enable nullable reference types by setting the nullable annotation context and nullable warning context to enable.
  * Use the `<Nullable>enable</Nullable>` setting in the *.csproj* file when creating new projects.
  * As we begin enabling nullable reference types in existing projects, add the `#nullable enable` pragma file-by-file when creating new files or touching existing files.
- Use the `?` suffix to explicitly declare the nullability of reference types.
- Consider using the `!` (null-forgiving) operator to supress compiler warnings when you know an expression can't be null.
- Avoid introducing new compiler warnings and address any existing warnings when refactoring code.
- Generate new database migrations when the nullable state of properties in entity model types change, requiring changes to the database model.

<a id="s2.2"></a>
### 2.2 Structure 

<a id="s2.2.1"></a>
#### 2.2.1 Entity Framework (EF) 

<a id="s2.2.1.1"></a>
##### 2.2.1.1 Annotations VS Fluent API

- Prefer to use Fluent API over annotations where we can
- Prefer to have one source of truth rather than split rules across different locations
- Follow the following convention:
    * Place all the configuration for an entity type inside a separate class implementing [`IEntityTypeConfiguration<TEntity>`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.ientitytypeconfiguration-1)
    * Make this class `internal` and place it in the same file as where the entity class itself is defined
    * All configurations within the assembly can then be registered from `OnModelCreating`:

        ```cs
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogEntityTypeConfiguration).Assembly);
        ```

<a id="s2.2.1.2"></a>
##### 2.2.1.2 Stored Procedures VS Linq VS SQL

- Our general guidance is we try to do everything with EF
- If we need additional performance or other SQL features unsupported by EF, we can consider generating SQL or a new stored procedure

<a id="s2.3"></a>
### 2.3 Best Practices 

<a id="s2.3.1"></a>
#### 2.3.1 Collections

- Prefer Sets over Lists.
- Use IList over ICollection for more useful features in tests.
- Use read-only collections like IReadOnlyList where applicable.
- Return IQueryable, not Queryable, when not wanting to materialise a query.

<a id="s2.3.2"></a>
#### 2.3.2 Entity Framework (EF)

- By default, use the synchronous versions of `Add`, `AddRange` etc. as opposed to the asynchronous versions `AddAsync`, `AddRangeAsync` etc.
    * The asynchronous versions appear to only be useful in specialist situations. The Microsoft documentation suggests that it is only useful when using the [`HiLoValueGenerator`](https://miro.com/app/board/o9J_ly21jhs=/?moveToWidget=3458764574909560501&cot=14)
- Prefer to use `context.{EntityType}.Add` over `context.Add`

<a id="s3"></a>
## 3 C# Testing Rules 

<a id="s3.1"></a>
### 3.1 Style 

<a id="s3.1.1"></a>
#### 3.1.1 Test Naming Convention

- Top-level class names will follow the naming convention `{ClassUnderTest}Tests`
- Create a nested class on a per-public-method being tested basis `{MethodUnderTest}Tests`
    * Do this from the outset, from the first method being tested. Do not wait until there are multiple methods being tested to convert to this style
    * Also use doubly-nested classes on a per-scenario basis where appropriate `{ScenarioUnderTest}Tests`. For example: 
             
      > Imagine under a nested `{MethodUnderTest}Tests`-type class, named `ListPublicationsTests`, we have methods like `UserIsPublicationOwner_Success`,
      `UserIsPublicationOwner_SupercededPublicationsExist`, `UserIsPublicationApprover_Success`, `UserIsPublicationApprover_SupercededPublicationsExist`
      etc... You might deem it sensible to create a 2nd level of nesting under `ListPublicationsTests` so that you then have the classes
      `UserIsPublicationOwnerTests` and `UserIsPublicationApproverTests`, and underneath them are their respective test methods like `Success`,
      `SupercededPublicationsExist`.
       
- Generally follow a `Scenario_Result` pattern for test method names
- Simple happy-path scenario could just be called `Success` for succinctness
  - When creating nested test classes within integration tests, have the base class extend the appropriate integration test fixture, and have the nested
    subclasses extend the base class so that they can access the base classes members. An example would look like:
  - 
    ```c#
      public class SignInControllerTests : IntegrationTest<TestStartup>
      {
          public SignInControllerTests(TestApplicationFactory<TestStartup> testApp) : base(testApp) {}

              public class RegistrationTests : SignInControllerTests
              {
                  public RegistrationTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
                  {
                  }

                  [Fact]
                  public async Task Success()
                  {
                      // Do something with the base class's "TestApp" member.
                      var client = TestApp
    ```

<a id="s3.1.2"></a>
#### 3.1.2 Separating common aspects of tests from the main test suites

There are common aspects that crosscut certain layers of the application code, and for these aspects, it is often cleaner and clearer for us to
separate those subsets of tests into separate test suites. Examples of this include Service Permissions
(such as [ReleaseServicePermissionTests](src/GovUk.Education.ExploreEducationStatistics.Admin.Tests/Services/ReleaseServicePermissionTests.cs))
and Caching-specific functionality
(such as [ReleaseControllerCachingTests](src/GovUk.Education.ExploreEducationStatistics.Content.Api.Tests/Controllers/ReleaseControllerCachingTests.cs)).

Typically, these themes can be identified when they follow a very similar style of test setup, execution and verification, and may also leverage
helper classes / utilities to reduce the boilerplate of these common patterns.

- Observe the current codebase conventions of separating out groups of tests from main test suite classes if they fall into the following categories:
  - Permission tests that use [UserService](src/GovUk.Education.ExploreEducationStatistics.Common/Services/Security/UserService.cs) as a basis for making
  decisions. These will typically be separated into test suite classes named `<Class>PermissionsTests` e.g. `ReleaseServicePermissionsTests`.
  - Caching tests that use an implementation of [ICacheService](src/GovUk.Education.ExploreEducationStatistics.Common/Services/Interfaces/ICacheService.cs)
  or test methods annotated with a [CacheAttribute](src/GovUk.Education.ExploreEducationStatistics.Common/Cache/CacheAttribute.cs). These will typically be
  separated into test suite classes named `<Class>CachingTests` e.g. `ReleaseControllerCachingTests`.
    - They will generally extend [CacheServiceTestFixture](src/GovUk.Education.ExploreEducationStatistics.Common.Tests/Fixtures/CacheServiceTestFixture.cs)
    in order to benefit from its lifecycle support for caching setup, teardown and control of static Services supplied to CacheAttributes.

<a id="s3.2"></a>
### 3.2 Test Setup 

<a id="s3.2.1"></a>
#### 3.2.1 Test Data Generators

Test Data Generators help to reduce the size and complexity of test data setup in test methods by supplying intelligent default values and helping
to maintain bidirectional relationships between entities to provide more realistic data setup.

- Prefer to use Test Data Generators for setting up entities if a Test Data Generator already exists for that entity e.g. use
[ReleaseGeneratorExtensions](src/GovUk.Education.ExploreEducationStatistics.Content.Model.Tests/Fixtures/ReleaseGeneratorExtensions.cs) to set up new 
[Release](src/GovUk.Education.ExploreEducationStatistics.Content.Model/Release.cs) instances in test setup.
- Consider breaking out new Test Data Generators for entities if they don't already exist if tackling test cases with complex data setups.
- Consider tackling the introduction and usage of Test Data Generators into separate pull requests if they risk overwhelming an existing branch
with additional changes.

<a id="s3.2.2"></a>
#### 3.2.2 Mocking

<a id="s3.2.2.1"></a>
#### 3.2.2.1 Creating Mocks

- Create Mocks individually in the test methods that will be using them.
- Have the individual test methods supply these Mocks to the construction of the class under test. Typically this will be done via a shared method 
within the test suite class called `Build<Type of Class Under Test>`. For testing a Service for example, Mocks would be supplied to a `BuildService` 
method within the test suite. The `BuildService` method in turn will allow as many or as few dependencies to be supplied to it using optional named 
parameters. See [PermalinkServiceTests](src/GovUk.Education.ExploreEducationStatistics.Data.Api.Tests/Services/PermalinkServiceTests.cs) for an example 
of usage.
  - Concrete examples of the naming convention for the construction methods so far include `BuildController`, `BuildService` and `BuildApp`.

<a id="s3.2.2.2"></a>
#### 3.2.2.2 When To Use Mocks

- Prefer using real dependencies rather than mocks for non-integration tests - unless the circumstances make this difficult.

<a id="s3.3"></a>
### 3.3 Best Practices 

<a id="s3.3.1"></a>
#### 3.3.1 Synchronous VS Asynchronous Test Methods

- If a test methods is using async method(s), then the method declaration should be marked as `async Task` or `async Task<T>`.

<a id="s3.3.2"></a>
#### 3.3.2 Integration VS Unit Tests

- Prefer integration tests at the controller level rather than tests at a lower level (i.e. service or repository).
