#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database;

/// <summary>
/// An extension of <see cref="SqlServerRetryingExecutionStrategy" /> that attempts to decide whether or not to retry
/// a database operation after a SQL exception has been encountered, based upon the text of its error message.
///
/// It firstly defers to its base class <see cref="SqlServerRetryingExecutionStrategy" /> to determine whether or not
/// the error can be retried by its Error Number alone.  If this cannot be determined, this class checks the error
/// message text itself for a match. 
/// </summary>
public class MessageAwareSqlServerRetryingExecutionStrategy : SqlServerRetryingExecutionStrategy
{
    /// <summary>
    /// An array of error message regular expressions, which we can use to determine whether or not we want to handle
    /// retries of SQL errors that are encountered.  If the SQL exception in question has a message that matches one of
    /// these regular expressions, we allow it to be retried.
    /// </summary>
    /// 
    /// <remarks>
    /// See <see cref="SqlServerTransientExceptionDetector.ShouldRetryOn" /> for full messages of some of the errors
    /// that we are handling here (Error Number 10060, for example).  In these examples,
    /// SqlServerTransientExceptionDetector is failing to handle these errors properly as it is not receiving the
    /// correct Error Number.  In the case of 10060, it is receiving Error Number 0 for example, which makes sense from
    /// the point of view that these Error Numbers are supposed to be issued from SQL Server itself, whereas with the
    /// example of 10060, SQL Server cannot actually be communicated with, and so Error Number 0 is actually issued
    /// from the SQL Client rather than the host itself.
    /// </remarks>
    private static readonly Regex[] TransientErrorMessages = {
        // This is the error message associated with Error Number 10060, but is reported with Error Number 0, and so
        // isn't handled correctly by SqlServerTransientExceptionDetector correctly.
        new(@"^A network-related or instance-specific error occurred while establishing a connection to SQL Server\."),
        
        // An error that can occur when the connection pool is exhausted for the target database.
        new(@"^A connection was successfully established with the server, but then an error occurred during the pre-login handshake\."),
      
        // An error that can occur when communication is interrupted during re-establishing communication with the database.
        new(@"^Connection Timeout Expired\.  The timeout period elapsed while attempting to consume the pre-login handshake acknowledgement\."),
    };

    private readonly ILogger _logger;

    public MessageAwareSqlServerRetryingExecutionStrategy(
        ExecutionStrategyDependencies dependencies, 
        int maxRetryCount, 
        TimeSpan maxRetryDelay) 
        : base(
            dependencies, 
            maxRetryCount, 
            maxRetryDelay,
            errorNumbersToAdd: null)
    {
        _logger = dependencies.Logger.Logger;
    }

    protected override bool ShouldRetryOn(Exception exception)
    {
        // Attempt to let Entity Framework's own SqlServerRetryingExecutionStrategy to handle the exception first.
        if (base.ShouldRetryOn(exception))
        {
            return true;
        }

        if (exception is not SqlException sqlException)
        {
            return false;
        }
        
        // Check the SQL exception for an error message that matches one of the messages that we have registered in
        // this class. 
        foreach (SqlError error in sqlException.Errors)
        {
            var matchingRegex =
                TransientErrorMessages.FirstOrDefault(messageRegex => messageRegex.IsMatch(error.Message));
                
            if (matchingRegex != null)
            {
                _logger.LogWarning($"SQL error can be retried - matches regex {matchingRegex}");
                return true;
            }
        }

        return false;
    }
}