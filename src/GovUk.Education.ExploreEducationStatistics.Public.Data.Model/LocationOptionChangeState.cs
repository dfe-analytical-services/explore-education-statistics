using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class LocationOptionChangeState
{
    public required int Id { get; set; }

    public required string PublicId { get; set; }

    public required string Label { get; set; }

    public required GeographicLevel Level { get; set; }

    protected abstract string Type { get; set; }

    protected string? Code { get; set; }

    protected string? OldCode { get; set; }

    protected string? Urn { get; set; }

    protected string? LaEstab { get; set; }

    protected string? Ukprn { get; set; }
}

public class LocationCodedOptionChangeState : LocationOptionChangeState, ILocationCodedOptionDetails
{
    public const string TypeValue = LocationCodedOptionMeta.TypeValue;

    protected override string Type { get; set; } = TypeValue;

    public new required string Code
    {
        get => base.Code ?? string.Empty;
        set => base.Code = value;
    }
}

public class LocationLocalAuthorityOptionChangeState : LocationOptionChangeState, ILocationLocalAuthorityOptionDetails
{
    public const string TypeValue = LocationLocalAuthorityOptionMeta.TypeValue;

    protected override string Type { get; set; } = TypeValue;

    public new required string Code
    {
        get => base.Code ?? string.Empty;
        set => base.Code = value;
    }

    public new required string OldCode
    {
        get => base.OldCode ?? string.Empty;
        set => base.OldCode = value;
    }
}

public class LocationProviderOptionChangeState : LocationOptionChangeState, ILocationProviderOptionDetails
{
    public const string TypeValue = LocationProviderOptionMeta.TypeValue;

    protected override string Type { get; set; } = TypeValue;

    public new required string Ukprn
    {
        get => base.Ukprn ?? string.Empty;
        set => base.Ukprn = value;
    }
}

public class LocationRscRegionOptionChangeState : LocationOptionChangeState, ILocationOptionDetails
{
    public const string TypeValue = LocationRscRegionOptionMeta.TypeValue;

    protected override string Type { get; set; } = TypeValue;
}

public class LocationSchoolOptionChangeState : LocationOptionChangeState, ILocationSchoolOptionDetails
{
    public const string TypeValue = LocationSchoolOptionMeta.TypeValue;

    protected override string Type { get; set; } = TypeValue;

    public new required string Urn
    {
        get => base.Urn ?? string.Empty;
        set => base.Urn = value;
    }

    public new required string LaEstab
    {
        get => base.LaEstab ?? string.Empty;
        set => base.LaEstab = value;
    }
}
