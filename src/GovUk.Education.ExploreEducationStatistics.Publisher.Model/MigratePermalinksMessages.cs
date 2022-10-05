#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public record PermalinksMigrationMessage;

public record PermalinkMigrationMessage(Guid PermalinkId);
