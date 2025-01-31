#nullable enable
#pragma warning disable EF1001
using System;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Database;

public class MessageAwareSqlServerRetryingExecutionStrategyTests
{
    private readonly MessageAwareSqlServerRetryingExecutionStrategy _strategy;

    public MessageAwareSqlServerRetryingExecutionStrategyTests()
    {
        var loggerFactory = new Mock<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>(Strict);

        loggerFactory
            .Setup(s => s.Logger)
            .Returns(Mock.Of<ILogger>());
        
        var dependencies = new ExecutionStrategyDependencies(
            Mock.Of<ICurrentDbContext>(),
            Mock.Of<IDbContextOptions>(),
            loggerFactory.Object);
        
        _strategy = new(
            dependencies, 
            maxRetryCount: 5, 
            maxRetryDelay: TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Exception_Unhandled()
    {
        var exception = CreateSqlException(
            "A random error message", 
            errorNumber: 0);

        var retry = (bool) typeof(MessageAwareSqlServerRetryingExecutionStrategy)
            .GetMethod("ShouldRetryOn", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_strategy, new object[] { exception })!;    
        
        // This SqlException should not be handled as it does not match any messages we handle and it doesn't match
        // any error numbers that the base SqlServerRetryingExecutionStrategy should handle.
        Assert.False(retry);
    }
    
    [Fact]
    public void Exception_HandledByBaseClass()
    {
        // This SqlException carries an Error Number that is handled in
        // SqlServerTransientExceptionDetector#ShouldRetryOn 
        var exception = CreateSqlException(
            "An error known by errorNumber to base class", 
            errorNumber: 10060);
        
        var retry = (bool) typeof(MessageAwareSqlServerRetryingExecutionStrategy)
            .GetMethod("ShouldRetryOn", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_strategy, new object[] { exception })!;    
        
        // This SqlException should be handled by the base class.
        Assert.True(retry);
    }
    
    [Fact]
    public void Exception_HandledByCustomStrategy()
    {
        // These SqlExceptions carry Error Messages that are handled by our custom strategy.
        var exception1 = CreateSqlException(
            "A network-related or instance-specific error occurred while establishing a connection to " +
            "SQL Server.", 
            errorNumber: 0);

        var exception2 = CreateSqlException(
            "A connection was successfully established with the server, but then an error occurred during " +
            "the pre-login handshake.", 
            errorNumber: 0);

        var exception3 = CreateSqlException(
            "Connection Timeout Expired.  The timeout period elapsed while attempting to consume the " +
            "pre-login handshake acknowledgement.", 
            errorNumber: 0);

        Assert.True((bool) typeof(MessageAwareSqlServerRetryingExecutionStrategy)
            .GetMethod("ShouldRetryOn", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_strategy, new object[] { exception1 })!);    
     
        Assert.True((bool) typeof(MessageAwareSqlServerRetryingExecutionStrategy)
            .GetMethod("ShouldRetryOn", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_strategy, new object[] { exception2 })!);  
        Assert.True((bool) typeof(MessageAwareSqlServerRetryingExecutionStrategy)
            .GetMethod("ShouldRetryOn", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(_strategy, new object[] { exception3 })!);  
    }
    
    private SqlException CreateSqlException(string errorMessage, int errorNumber = 0)
    {
        var collection = Construct<SqlErrorCollection>();
        
        var error = Construct<SqlError>(
            errorNumber, // Number
            (byte)2, // State
            (byte)3, // Class
            "server name", // Server
            errorMessage, // Message
            "proc", // Procedure
            100, // LineNumber
            (uint)1, // Win32ErrorCode
            null!); // Exception

        typeof(SqlErrorCollection)
            .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(collection, new object[] { error });    
    
        return (typeof(SqlException)
            .GetMethod(
                "CreateException", 
                BindingFlags.NonPublic | BindingFlags.Static, 
                null, 
                CallingConventions.ExplicitThis, 
                new[] { typeof(SqlErrorCollection), typeof(string) }, 
                new ParameterModifier[] { })!
            .Invoke(null, new object[] { collection, "11.0.0" }) as SqlException)!;
    }

    private T Construct<T>(params object?[] p)
    {
        return (T)typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0].Invoke(p);
    }
}