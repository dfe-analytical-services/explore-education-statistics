namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Interfaces;

public interface ILocationOptionDetails
{
    string PublicId { get; set; }

    string Label { get; set; }
}

public interface ILocationCodedOptionDetails : ILocationOptionDetails
{
    string Code { get; set; }
}

public interface ILocationLocalAuthorityOptionDetails : ILocationOptionDetails
{
    string Code { get; set; }

    string OldCode { get; set; }
}

public interface ILocationSchoolOptionDetails : ILocationOptionDetails
{
    string Urn { get; set; }

    string LaEstab { get; set; }
}

public interface ILocationProviderOptionDetails : ILocationOptionDetails
{
    string Ukprn { get; set; }
}
