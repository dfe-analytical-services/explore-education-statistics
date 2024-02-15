namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class LocationOptionMetaBase
{
    public required string PublicId { get; set; }

    public required int PrivateId { get; set; }

    public required string Label { get; set; }
}

public class LocationOptionMeta : LocationOptionMetaBase
{
    public required string Code { get; set; }
}

public class LocationLocalAuthorityOptionMeta : LocationOptionMetaBase
{
    public required string Code { get; set; }

    public required string OldCode { get; set; }
}

public class LocationProviderOptionMeta : LocationOptionMetaBase
{
    public required string Ukprn { get; set; }
}

public class LocationRscRegionOptionMeta : LocationOptionMetaBase;

public class LocationSchoolOptionMeta : LocationOptionMetaBase
{
    public required string Urn { get; set; }

    public required string LaEstab { get; set; }
}
