using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

public record ImportMessage(Guid Id);

public record CancelImportMessage(Guid Id);

public record RestartImportsMessage;