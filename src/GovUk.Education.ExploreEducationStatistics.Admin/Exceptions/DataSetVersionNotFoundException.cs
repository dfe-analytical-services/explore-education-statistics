using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Exceptions; 

public class DataSetVersionNotFoundException(string message) : InvalidOperationException(message);
