using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<CommentState>))]
    public enum CommentState
    {
        open,
        resolved
    }
}