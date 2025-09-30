namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Tests;

public abstract class SqidEncoderTests
{
    public class EncodeTests() : SqidEncoderTests
    {
        [Fact]
        public void Success()
        {
            var encodedNumber = SqidEncoder.Encode(1);

            Assert.Equal("dP0Zw", encodedNumber);
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

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("-")]
        [InlineData("!")]
        [InlineData("#")]
        [InlineData(":")]
        [InlineData(",")]
        [InlineData("abc!")]
        [InlineData("abc ")]
        [InlineData(" abc")]
        public void InvalidSqid_Throws(string invalidSqid)
        {
            Assert.Throws<InvalidOperationException>(() =>
                SqidEncoder.DecodeSingleNumber(invalidSqid)
            );
        }

        [Theory]
        [InlineData("emuRS")] // represents [1, 1]
        [InlineData("mU9kKE")] // represents [1, 1, 1]
        public void SqidRepresentsMultipleNumbers_Throws(string sqid)
        {
            Assert.Throws<InvalidOperationException>(() => SqidEncoder.DecodeSingleNumber(sqid));
        }
    }

    public class TryDecodeSingleNumberTests() : SqidEncoderTests
    {
        [Fact]
        public void SingleEncodedNumber_SuccessfullyDecodes()
        {
            var encodedNumber = SqidEncoder.Encode(1);

            var successful = SqidEncoder.TryDecodeSingleNumber(
                encodedNumber,
                out var decodedNumber
            );

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

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("-")]
        [InlineData("!")]
        [InlineData("#")]
        [InlineData(":")]
        [InlineData(",")]
        [InlineData("abc!")]
        [InlineData("abc ")]
        [InlineData(" abc")]
        public void InvalidSqid_ReturnsFalse(string invalidSqid)
        {
            var successful = SqidEncoder.TryDecodeSingleNumber(invalidSqid, out var decodedNumber);

            Assert.False(successful);
            Assert.Equal(0, decodedNumber);
        }

        [Theory]
        [InlineData("emuRS")] // represents [1, 1]
        [InlineData("mU9kKE")] // represents [1, 1, 1]
        public void SqidRepresentsMultipleNumbers_ReturnsFalse(string sqid)
        {
            var successful = SqidEncoder.TryDecodeSingleNumber(sqid, out var decodedNumber);

            Assert.False(successful);
            Assert.Equal(0, decodedNumber);
        }
    }
}
