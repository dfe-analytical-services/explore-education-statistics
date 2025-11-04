using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IPostgreSqlRepository
{
    /// <summary>
    /// Determines if a given key exists within the JSON at the specified JSONB path.
    /// If the JSONB path does not exist, this method will return false.
    /// </summary>
    Task<bool> KeyExistsAtJsonbPath<TDbContext, TRowId>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        string keyValue,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext;

    /// <summary>
    /// Retrieve a JSON fragment from a JSONB column in a particular table and row,
    /// given a JSON path to the fragment.
    /// </summary>
    Task<TResponse?> GetJsonbFromPath<TDbContext, TRowId, TResponse>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TResponse : class;

    /// <summary>
    /// Set the JSON to the given value within a JSONB column in a particular table and row,
    /// given a JSON path to the fragment that needs setting.
    /// </summary>
    Task<TValue?> SetJsonbAtPath<TDbContext, TValue, TRowId>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        TValue? value,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TValue : class;

    /// <summary>
    /// Update the JSON within a JSONB column in a particular table and row,
    /// given a JSON path to the fragment that needs updating.
    ///
    /// The "updateValueFn" function is given the existing JSON at the specified path if it
    /// exists, and can then transform it and return a succeeding Either.  Optionally it can
    /// fail and return a failing Either.
    /// </summary>
    Task<Either<TFailure, TValue?>> UpdateJsonbAtPath<TDbContext, TValue, TRowId, TFailure>(
        TDbContext context,
        JsonbPathRequest<TRowId> request,
        Func<TValue?, Task<Either<TFailure, TValue?>>> updateValueFn,
        CancellationToken cancellationToken = default
    )
        where TDbContext : DbContext
        where TValue : class;
}
