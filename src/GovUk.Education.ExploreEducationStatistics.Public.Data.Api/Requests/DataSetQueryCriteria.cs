using System.Text.Json.Serialization;
using FluentValidation.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests.Converters;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

[JsonConverter(typeof(DataSetQueryCriteriaJsonConverter))]
[SwaggerSubType(typeof(DataSetQueryCriteriaAnd))]
[SwaggerSubType(typeof(DataSetQueryCriteriaOr))]
[SwaggerSubType(typeof(DataSetQueryCriteriaNot))]
[SwaggerSubType(typeof(DataSetQueryCriteriaFacets))]
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
