#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

/// <summary>
/// Used as a DTO by <see cref="PostgreSqlRepository" /> to extract arbitrary JSON fragments from JSONB columns.
/// In order to use this in a DbContext, to must be registered as a DTO by adding the following configuration to
/// the OnModelBuilding() method of the DbContext:
/// <code>
/// modelBuilder.Entity&lt;JsonFragment&gt;().HasNoKey().ToView(null);
/// </code>
/// </summary>
public record JsonFragment(string? JsonString);
