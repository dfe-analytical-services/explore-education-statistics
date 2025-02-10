using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Options;

public record NewVersionEmailTemplateIdPicker(
    string MajorVersionTemplateId,
    string MinorVersionTemplateId)
{
    public string GetTemplateId(string version)
    { 
        if(!DataSetVersionNumber.TryParse(version, out var dataSetVersionNumber))
        {
            throw new System.ArgumentException("The data set version version number supplied is invalid.");
        }
        var isNewMajorVersion = dataSetVersionNumber!.Major >= 2
            && dataSetVersionNumber.Patch == 0
            && dataSetVersionNumber.Minor == 0;
        return isNewMajorVersion ? MajorVersionTemplateId : MinorVersionTemplateId;
    }
}
