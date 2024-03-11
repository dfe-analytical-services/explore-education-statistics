using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using Sqids;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Utils;

public abstract class SqidEncoderTests
{
    private readonly SqidsEncoder<int> _sqidsEncoder = new(new()
    {
        Alphabet = "UjIo1rQXDHTmNsy6p4SlAtO0uqhfCiexFMVc5nG3v2Bbz8dKgWJRa79PZYwkEL",
        MinLength = 5,
    });

    public class EncodeTests() : SqidEncoderTests
    {
        [Fact]
        public void Success()
        {
            var encodedNumber = SqidEncoder.Encode(1);

            var expected = _sqidsEncoder.Encode(1);

            Assert.Equal(expected, encodedNumber);
        }
    }

    public class DecodeSingleNumberTests() : SqidEncoderTests
    {
        [Fact]
        public void SingleEncodedNumber_SuccessfullyDecodes()
        {
            var encodedNumber = SqidEncoder.Encode(1);

            var decodedSqid = SqidEncoder.DecodeSingleNumber(encodedNumber);

            Assert.Equal(1, decodedSqid);
        }

        [Fact]
        public void CanonicalAndNonCanonicalIdPair_SuccessfullyDecodesToSameNumber()
        {
            var nonCanonicalId = "UU";
            var canonicalId = "wwbxT";

            var decodedNonCanonicalId = SqidEncoder.DecodeSingleNumber(nonCanonicalId);
            var decodedCanonicalId = SqidEncoder.DecodeSingleNumber(canonicalId);

            Assert.Equal(60, decodedNonCanonicalId);
            Assert.Equal(60, decodedCanonicalId);
        }

        [Fact]
        public void InvalidSqid_Throws()
        {
            var invalidSqid = "-";

            Assert.Throws<InvalidOperationException>(() => SqidEncoder.DecodeSingleNumber(invalidSqid));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 1, 1)]
        public void SqidRepresentsMultipleNumbers_Throws(params int[] numbers)
        {
            var encodedNumber = _sqidsEncoder.Encode(numbers);

            Assert.Throws<InvalidOperationException>(() => SqidEncoder.DecodeSingleNumber(encodedNumber));
        }
    }

    public class TryDecodeSingleNumberTests() : SqidEncoderTests
    {
        [Fact]
        public void SingleEncodedNumber_SuccessfullyDecodes()
        {
            var encodedNumber = SqidEncoder.Encode(1);

            var successful = SqidEncoder.TryDecodeSingleNumber(encodedNumber, out var decodedNumber);

            Assert.True(successful);
            Assert.Equal(1, decodedNumber);
        }

        [Fact]
        public void CanonicalId_SuccessfullyDecodes()
        {
            var canonicalId = "wwbxT";

            var successful = SqidEncoder.TryDecodeSingleNumber(canonicalId, out var decodedNumber);

            Assert.True(successful);
            Assert.Equal(60, decodedNumber);
        }

        [Fact]
        public void NonCanonicalId_ReturnsFalse()
        {
            var canonicalId = "UU";

            var successful = SqidEncoder.TryDecodeSingleNumber(canonicalId, out var decodedNumber);

            Assert.False(successful);
            Assert.Equal(0, decodedNumber);
        }

        [Fact]
        public void InvalidSqid_ReturnsFalse()
        {
            var invalidSqid = "-";

            var successful = SqidEncoder.TryDecodeSingleNumber(invalidSqid, out var decodedNumber);

            Assert.False(successful);
            Assert.Equal(0, decodedNumber);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 1, 1)]
        public void SqidRepresentsMultipleNumbers_Throws(params int[] numbers)
        {
            var encodedNumber = _sqidsEncoder.Encode(numbers);

            var successful = SqidEncoder.TryDecodeSingleNumber(encodedNumber, out var decodedNumber);

            Assert.False(successful);
            Assert.Equal(0, decodedNumber);
        }
    }
}
