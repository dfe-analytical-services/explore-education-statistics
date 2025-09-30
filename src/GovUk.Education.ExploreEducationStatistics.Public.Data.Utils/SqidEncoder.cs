using Sqids;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;

public static class SqidEncoder
{
    private static readonly SqidsEncoder<int> _encoder = new(
        new()
        {
            Alphabet = "UjIo1rQXDHTmNsy6p4SlAtO0uqhfCiexFMVc5nG3v2Bbz8dKgWJRa79PZYwkEL",
            MinLength = 5,
        }
    );

    public static string Encode(int number) => _encoder.Encode(number);

    public static int DecodeSingleNumber(string sqid) => _encoder.Decode(sqid).Single();

    public static bool TryDecodeSingleNumber(string sqid, out int decodedId)
    {
        if (_encoder.Decode(sqid) is [var id] && sqid == Encode(id))
        {
            decodedId = id;
            return true;
        }

        decodedId = default;
        return false;
    }
}
