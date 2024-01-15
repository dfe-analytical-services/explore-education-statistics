using NanoidDotNet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Utils;

public class ShortId
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const byte Size = 10;

    private readonly Random _random;

    private readonly HashSet<string> _existingIds = [];
    private readonly bool _checkCollisions;

    public ShortId(
        int? seed = null,
        bool checkCollisions = false)
    {
        _checkCollisions = checkCollisions;
        _random = seed is not null ? new Random(seed.Value) : new CryptoRandom();
    }

    private string NextId() => Nanoid.Generate(random: _random, alphabet: Alphabet, size: Size);
    
    public string Generate()
    {
        var id = NextId();

        if (!_checkCollisions)
        {
            return id;
        }

        while (_existingIds.Contains(id))
        {
            id = NextId();
        }

        _existingIds.Add(id);

        return id;
    }
}
