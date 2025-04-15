using System.Text.Json;
using System.Text.Json.Serialization;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;

public class SemVersionJsonConverter : JsonConverter<SemVersion>
{
    public override SemVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var versionString = reader.GetString();
        return SemVersion.Parse(versionString,  SemVersionStyles.OptionalMinorPatch
                                                | SemVersionStyles.AllowWhitespace
                                                | SemVersionStyles.AllowLowerV); // Parse the version string into a SemVersion object
    }

    public override void Write(Utf8JsonWriter writer, SemVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString()); // Serialize the SemVersion as a string
    }
}
