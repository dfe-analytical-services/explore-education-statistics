#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;

public class EmailSendFailedException(string message, Exception? inner = null) : Exception(message, inner) { }
