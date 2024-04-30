using System.Text.Json.Serialization;
using FluentValidation.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

[JsonConverter(typeof(DataSetQueryCriteriaJsonConverter))]
public abstract record DataSetQueryCriteria
{
    protected static void InheritanceValidator<TCriteria>(
        PolymorphicValidator<TCriteria, DataSetQueryCriteria> validator)
        where TCriteria : DataSetQueryCriteria
    {
        validator.Add(_ => new DataSetQueryCriteriaAnd.Validator());
        validator.Add(_ => new DataSetQueryCriteriaOr.Validator());
        validator.Add(_ => new DataSetQueryCriteriaNot.Validator());
        validator.Add(_ => new DataSetQueryCriteriaFacets.Validator());
    }
}
