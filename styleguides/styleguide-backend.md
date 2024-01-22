# Explore Education Statistics - Backend Style Guide

<details markdown="1">
  <summary>Table of Contents</summary>

-   [1 Background](#s1)
-   [2 C# Language Rules](#s2)
    *   [2.1 Style](#s2.1)
        +   [2.1.1 Named Parameters](#s2.1.1)
    *   [2.2 Structure](#s2.2)
        +   [2.2.1 Entity Framework (EF)](#s2.2.1)
            +   [2.2.1.1 Annotations VS Fluent API](#s2.2.1.1)
            +   [2.2.1.2 Stored Procedures VS Linq VS SQL](#s2.2.1.2)
    *   [2.3 Best Practices](#s2.3)
        +   [2.3.2 Entity Framework (EF)](#s2.3.2)
-   [3 C# Testing Rules](#s3)
    *   [3.1 Style](#s3.1)
        +   [3.1.1 Test Naming Conventon](#s3.1.1)
    *   [3.2 Test Setup](#s3.2)
    *   [3.3 Best Practices](#s3.3)
        +   [3.3.1 Synchronous VS Asynchronous Test Methods](#s3.3.1)

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
#### 2.1.1 Named Parameters

- As a rule, if we hit 3 or more parameters for a method call, consider using named parameters, or break out a class/record to hold the parameters*
- If we have multiple parameters of the same type, use named parameters to protect against the accidental reordering of parameters*

*This includes constructors

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

<a id="s2.3.2"></a>
#### 2.3.2 Entity Framework (EF)

- By default, use the synchronous versions of `Add`, `AddRange` etc. as opposed to the asynchronous versions `AddAsync`, `AddRangeAsync` etc.
    * The asynchronous versions appear to only be useful in specialist situations. The Microsoft documentation suggests that it is only useful when using the [`HiLoValueGenerator`](https://miro.com/app/board/o9J_ly21jhs=/?moveToWidget=3458764574909560501&cot=14)
- Prefer to use `context.{EntityType}.Add` over `context.Add`


<a id="s3"></a>
## 2 C# Testing Rules 

<a id="s3.1"></a>
### 3.1 Style 

<a id="s3.1.1"></a>
#### 3.1.1 Test Naming Convention

- Top-level class names will follow the naming convention `{ClassUnderTest}Tests`
- Create a nested class on a per-public-method being tested basis `{MethodUnderTest}Tests`
    * Do this from the outset, from the first method being tested. Do not wait until there are multiple methods being tested to convert to this style
    * Also use doubly-nested classes on a per-scenario basis where appropriate `{ScenarioUnderTest}Tests`. For example: 
             
      > Imagine under a nested `{MethodUnderTest}Tests`-type class, named `ListPublicationsTests`, we have methods like `UserIsPublicationOwner_Success`, `UserIsPublicationOwner_SupercededPublicationsExist`, `UserIsPublicationApprover_Success`, `UserIsPublicationApprover_SupercededPublicationsExist` etc... You might deem it sensible to create a 2nd level of nesting under `ListPublicationsTests` so that you then have the classes `UserIsPublicationOwnerTests` and `UserIsPublicationApproverTests`, and underneath them are their respective test methods like `Success`, `SupercededPublicationsExist`.   
       
- Generally follow a `Scenario_Result` pattern for test method names
- Simple happy-path scenario could just be called `Success` for succinctness


<a id="s3.2"></a>
### 3.2 Test Setup 

<a id="s3.3"></a>
### 3.3 Best Practices 

<a id="s3.3.1"></a>
#### 3.3.1 Synchronous VS Asynchronous Test Methods

- If a test methods is using async method(s), then the method declaration should be marked as `async Task` or `async Task<T>`.
