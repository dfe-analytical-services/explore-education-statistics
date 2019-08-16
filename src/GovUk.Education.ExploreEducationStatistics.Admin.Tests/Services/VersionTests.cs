using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class VersionTests
    {
        [Fact]
        public void VersionOrderingEmpty()
        {
            var versioned = new Versioned();
            Assert.Null(versioned.Current);
            Assert.Null(versioned.Latest);
            Assert.Null(versioned.First);
            Assert.NotNull(versioned.Ordered);
            Assert.Empty(versioned.Ordered);
        }
        
        [Fact]
        public void VersionOrderingAllFuture()
        {
            var versioned = new Versioned
            {
                Versions = new List<Version>
                {
                    new Version
                    {
                        Created = DateTime.Now.AddDays(20), // 20 days in the future
                        Name = "+20days"
                    }
                }
            };
            Assert.Null(versioned.Current);
            Assert.Equal("+20days", versioned.Latest.Name);
            Assert.Equal("+20days", versioned.First.Name);
        }
        
        [Fact]
        public void VersionOrdering()
        {
            var versioned = new Versioned
            {
                Versions = new List<Version>
                {
                    new Version
                    {
                        Created = DateTime.Now.AddDays(-10), // 10 days ago.
                        Name = "-10days" 
                    },
                    new Version
                    {
                        Created = DateTime.Now.AddDays(-20),  // 20 days ago.
                        Name = "-20days"
                    },
                    new Version
                    {
                        Created = DateTime.Now.AddDays(20), // 20 days in the future
                        Name = "+20days"
                    }
                }
            };
            Assert.Equal("-10days", versioned.Current.Name);
            Assert.Equal("+20days", versioned.Latest.Name);
            Assert.Equal("-20days", versioned.First.Name);
            Assert.Equal("+20days", versioned.Ordered[2].Name);
            Assert.Equal("-10days", versioned.Ordered[1].Name);
            Assert.Equal("-20days", versioned.Ordered[0].Name);
            Assert.Equal("-20days", versioned.Ordered[0].Name);
        }
        
    }
    
    

    public class Version : IVersion
    {
        public DateTime Created { get; set; }

        public string Name { get; set; }
    }

    public class Versioned : AbstractVersioned<Version>
    {
        
    }
    
}