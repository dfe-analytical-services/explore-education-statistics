#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public abstract class UuidUtilsTests
{
    private static readonly Guid DnsNamespace = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
    private static readonly Guid UrlNamespace = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    public class UuidV5Tests : UuidUtilsTests
    {
        [Theory]
        [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8", "www.example.com", "2ed6657d-e927-568b-95e1-2665a8aea6a2")]
        [InlineData(
            "6ba7b811-9dad-11d1-80b4-00c04fd430c8",
            "https://www.example.com/path?q=é",
            "0ca8ed7d-15e6-55d7-907f-ea05fc370196"
        )]
        public void MatchesKnownValues(string namespaceId, string name, string expected)
        {
            Assert.Equal(Guid.Parse(expected), UuidUtils.UuidV5(Guid.Parse(namespaceId), name));
        }

        [Fact]
        public void SameForSameNamespaceAndName()
        {
            Assert.Equal(UuidUtils.UuidV5(DnsNamespace, "name"), UuidUtils.UuidV5(DnsNamespace, "name"));
        }

        [Fact]
        public void DifferentForDifferentNamespace()
        {
            Assert.NotEqual(UuidUtils.UuidV5(DnsNamespace, "name"), UuidUtils.UuidV5(UrlNamespace, "name"));
        }

        [Fact]
        public void DifferentForDifferentName()
        {
            Assert.NotEqual(UuidUtils.UuidV5(DnsNamespace, "name"), UuidUtils.UuidV5(DnsNamespace, "Name"));
        }

        [Fact]
        public void HasVersion5AndRfcVariant()
        {
            var uuid = UuidUtils.UuidV5(DnsNamespace, "name");

            Assert.Equal(5, uuid.Version);
            Assert.Equal(0b10, uuid.Variant >> 2);
        }
    }
}
