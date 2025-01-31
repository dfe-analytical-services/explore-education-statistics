#nullable enable
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

/// <summary>
/// Provides content as a JSON string serialised using <see cref="Newtonsoft.Json"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class is not to be confused with <see cref="System.Net.Http.Json.JsonContent"/>,
/// which is intended for use with the newer <see cref="System.Text.Json"/> APIs.
/// </para>
/// <para>
/// We will need remove this in favour of <see cref="System.Net.Http.Json.JsonContent"/>
/// if we ever intend migrate to <see cref="System.Text.Json"/>.
/// </para>
/// </remarks>
public class JsonNetContent : StringContent
{
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;
    private const string MediaType = MediaTypeNames.Application.Json;

    public JsonNetContent(object content, JsonSerializerSettings? settings = null, Encoding? encoding = null) : base(
        content: JsonConvert.SerializeObject(content, settings),
        encoding: encoding ?? DefaultEncoding,
        mediaType: MediaType)
    {
    }
}
