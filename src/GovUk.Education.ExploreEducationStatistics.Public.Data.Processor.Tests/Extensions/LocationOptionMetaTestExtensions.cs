using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;

public static class LocationOptionMetaTestExtensions
{
    public static void AssertEqual(this LocationOptionMeta expectedOption, ParquetLocationOption actualOption)
    {
        Assert.Equal(expectedOption.Label, actualOption.Label);
        Assert.Equal(SqidEncoder.Encode(expectedOption.Id), actualOption.PublicId);

        switch (expectedOption)
        {
            case LocationCodedOptionMeta codedOption:
                Assert.Equal(codedOption.Code, actualOption.Code);
                Assert.Null(actualOption.OldCode);
                Assert.Null(actualOption.Ukprn);
                Assert.Null(actualOption.Urn);
                Assert.Null(actualOption.LaEstab);
                break;
            case LocationLocalAuthorityOptionMeta laOption:
                Assert.Equal(laOption.Code, actualOption.Code);
                Assert.Equal(laOption.OldCode, actualOption.OldCode);
                Assert.Null(actualOption.Ukprn);
                Assert.Null(actualOption.Urn);
                Assert.Null(actualOption.LaEstab);
                break;
            case LocationProviderOptionMeta providerOption:
                Assert.Null(actualOption.Code);
                Assert.Null(actualOption.OldCode);
                Assert.Equal(providerOption.Ukprn, actualOption.Ukprn);
                Assert.Null(actualOption.Urn);
                Assert.Null(actualOption.LaEstab);
                break;
            case LocationRscRegionOptionMeta:
                Assert.Null(actualOption.Code);
                Assert.Null(actualOption.OldCode);
                Assert.Null(actualOption.Ukprn);
                Assert.Null(actualOption.Urn);
                Assert.Null(actualOption.LaEstab);
                break;
            case LocationSchoolOptionMeta schoolOption:
                Assert.Null(actualOption.Code);
                Assert.Null(actualOption.OldCode);
                Assert.Null(actualOption.Ukprn);
                Assert.Equal(schoolOption.Urn, actualOption.Urn);
                Assert.Equal(schoolOption.LaEstab, actualOption.LaEstab);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(expectedOption),
                    $"Unsupported {expectedOption.GetType().Name} type"
                );
        }
    }
}
