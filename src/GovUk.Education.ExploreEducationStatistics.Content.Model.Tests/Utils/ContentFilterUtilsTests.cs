#nullable enable
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils
{
    public class ContentFilterUtilsTests
    {
        public class CommentsFilterPattern
        {
            private readonly Regex _regex = new(ContentFilterUtils.CommentsFilterPattern, RegexOptions.Compiled);

            [Fact]
            public void Replace_TypicalFormat()
            {
                var content = @"
                    <comment-start name=""comment-1""></comment-start>Content 1<comment-end name=""comment-1""></comment-end>
                    <commentplaceholder-start name=""comment-2""></commentplaceholder-start>Content 2<commentplaceholder-end name=""comment-2""></commentplaceholder-end>
                    <resolvedcomment-start name=""comment-3""></resolvedcomment-start>Content 3<resolvedcomment-end name=""comment-3""></resolvedcomment-end>".TrimIndent();

                Assert.Equal(
                    @"
                    Content 1
                    Content 2
                    Content 3".TrimIndent(),
                    _regex.Replace(content, string.Empty)
                );
            }

            [Fact]
            public void Replace_SelfClosing()
            {
                var content = @"
                    <comment-start name=""comment-1""/>Content 1<comment-end name=""comment-1""/>
                    <commentplaceholder-start name=""comment-2""/>Content 2<commentplaceholder-end name=""comment-2""/>
                    <resolvedcomment-start name=""comment-3""/>Content 3<resolvedcomment-end name=""comment-3""/>".TrimIndent();

                Assert.Equal(
                    @"
                    Content 1
                    Content 2
                    Content 3".TrimIndent(),
                    _regex.Replace(content, string.Empty)
                );
            }

            [Fact]
            public void Replace_WithWhitespaces()
            {
                var content = @"
                    <comment-start    name=""comment-1    ""   >   </ comment-start>Content 1<comment-end     name=""comment-1  ""  >  </   comment-end>
                    <comment-start    name=""comment-2    ""   >   </ comment-start>
                    Content 2
                    <comment-end     name=""comment-2  ""  >  </   comment-end>
                    <comment-start    name=""comment-3    ""   />
                    Content 3
                    <comment-end     name=""comment-3  ""  />
                    <comment-start 
                        name=""comment-4""
                    ></comment-start>
                    Content 4
                    <comment-end 
                        name=""comment-4""
                    ></comment-end>
                    <comment-start 
                        name=""comment-5""
                    />
                    Content 5
                    <comment-end 
                        name=""comment-5""
                    />
                    ".TrimIndent();


                Assert.Equal(
                    @"
                Content 1

                Content 2


                Content 3

                
                Content 4


                Content 5

                ".TrimIndent(),
                    _regex.Replace(content, string.Empty)
                );
            }

            [Fact]
            public void Replace_WithMarkup()
            {
                var content = @"
                    <p>
                        Content 1 <comment-start name=""comment-1""></comment-start>goes here<comment-end name=""comment-1""></comment-end>
                    </p>
                    <ul>
                        <li><comment-start name=""comment-2""></comment-start>Content 2<comment-end name=""comment-2""></comment-end></li>
                        <li><commentplaceholder-start name=""comment-3""></commentplaceholder-start>Content 3<commentplaceholder-end name=""comment-3""></commentplaceholder-end></li>
                        <li><resolvedcomment-start name=""comment-4""></resolvedcomment-start>Content 4<resolvedcomment-end name=""comment-4""></resolvedcomment-end></li>
                    </ul>".TrimIndent();


                Assert.Equal(
                    @"
                    <p>
                        Content 1 goes here
                    </p>
                    <ul>
                        <li>Content 2</li>
                        <li>Content 3</li>
                        <li>Content 4</li>
                    </ul>".TrimIndent(),
                    _regex.Replace(content, string.Empty)
                );
            }

            [Fact]
            public void Replace_WithWhitespacesAndMarkup()
            {
                var content = @"
                    <p>
                        Content 1 <comment-start    name=""comment-1  ""></comment-start  >goes here<comment-end   name=  ""comment-1""  ></comment-end   >
                    </p>
                    <ul>
                        <li><   comment-start   name=""  comment-2""  ></comment-start  >Content 2<comment-end   name=""  comment-2""></comment-end   ></li>
                        <li>
                            <commentplaceholder-start 
                                name=""comment-3""
                            ></commentplaceholder-start
                            >
                            Content 3
                            <commentplaceholder-end 
                                name=""comment-3""
                            ></commentplaceholder-end
                            >
                        </li>
                        <li>
                            <resolvedcomment-start    name=""comment-4""    />
                                Content 4
                            <resolvedcomment-end   name=""comment-4""   />
                        </li>
                    </ul>".TrimIndent();

                var replaced = _regex.Replace(content, string.Empty);
                Assert.Equal(
                    @"
                    <p>
                        Content 1 goes here
                    </p>
                    <ul>
                        <li>Content 2</li>
                        <li>
                            
                            Content 3
                            
                        </li>
                        <li>
                            
                                Content 4
                            
                        </li>
                    </ul>".TrimIndent(),
                    replaced
                );
            }
        }
    }
}