#nullable enable
using System.Collections.Generic;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests
{
    public class LocationAttributeNodeTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Region _eastMidlands = new("E12000004", "East Midlands");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");
        private readonly LocalAuthority _nottingham = new("E06000018", "", "Nottingham");

        [Fact]
        public void IsLeaf_ReturnsTrueWithoutChildren()
        {
            var node = new LocationAttributeNode(_england);
            Assert.True(node.IsLeaf);
        }

        [Fact]
        public void IsLeaf_ReturnsFalseWithChildren()
        {
            var node = new LocationAttributeNode(_england)
            {
                Children = new List<LocationAttributeNode>
                {
                    new(_eastMidlands)
                }
            };
            Assert.False(node.IsLeaf);
        }

        [Fact]
        public void GetLeafAttributes_ReturnsOwnAttributeWhenLeaf()
        {
            var node = new LocationAttributeNode(_england);

            Assert.True(node.IsLeaf);

            var result = node.GetLeafAttributes();
            var leafAttribute = Assert.Single(result);
            Assert.Equal(_england, leafAttribute);
        }

        [Fact]
        public void GetLeafAttributes()
        {
            var node = new LocationAttributeNode(_england)
            {
                Children = new List<LocationAttributeNode>
                {
                    new(_eastMidlands)
                    {
                        Children = new List<LocationAttributeNode>
                        {
                            new(_derby),
                            new(_nottingham)
                        }
                    }
                }
            };

            var result = node.GetLeafAttributes();
            Assert.Equal(2, result.Count);
            Assert.Equal(_derby, result[0]);
            Assert.Equal(_nottingham, result[1]);
        }
    }
}
