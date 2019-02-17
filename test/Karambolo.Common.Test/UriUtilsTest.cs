using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Karambolo.Common
{
    public class UriUtilsTest
    {
        [Fact]
        public void BuildPathTest()
        {
            Assert.Equal("", UriUtils.BuildPath());
            Assert.Equal("path/to/resource", UriUtils.BuildPath("path", "to", "resource"));
            Assert.Equal("path/to/resource/", UriUtils.BuildPath("path", "to", "resource/"));
            Assert.Equal("path/to/resource", UriUtils.BuildPath("path/", "/to", "resource"));
            Assert.Equal("/path/to/resource", UriUtils.BuildPath("/path", "to", "resource"));
            Assert.Equal("path/to/resource", UriUtils.BuildPath(null, "/path", null, "/to", string.Empty, "/resource"));
            Assert.Equal("/path//to/resource", UriUtils.BuildPath("/", "/path", "//", "/to", "/", "/resource"));
        }

        [Fact]
        public void BuildQueryTest()
        {
            Assert.Equal("", UriUtils.BuildQuery(new { }));
            Assert.Equal("", UriUtils.BuildQuery(new Dictionary<string, object> { }));

            Assert.Equal("?value=zero", UriUtils.BuildQuery(new { value = "zero" }));
            Assert.Equal("?value=zero", UriUtils.BuildQuery(new Dictionary<string, object> { ["value"] = "zero" }));

            Assert.Equal("?key=0&value=zero", UriUtils.BuildQuery(new { key = 0, value = "zero" }));
            Assert.Equal("?key=0&value=zero", UriUtils.BuildQuery(new Dictionary<string, object> { ["key"] = 0, ["value"] = "zero" }));

            Assert.Equal("?value=1&value=2&value=3", UriUtils.BuildQuery(new { value = new[] { 1, 2, 3 } }));

            Assert.Equal("?value=spec%3F%26l", UriUtils.BuildQuery(new { value = "spec?&l" }));
        }

        [Fact]
        public void BuildUrlTest()
        {
            Assert.Equal("/path/to/resource/?key=0&value=zero", UriUtils.BuildUrl(new[] {"/path", "to", "resource/" }, new { key = 0, value = "zero" }));
            Assert.Equal("/path/to/resource/?value=1&value=2&value=3", UriUtils.BuildUrl(new[] { "/path/", "/to/", "/resource/" }, new Dictionary<string, object> { ["value"] = new[] { 1, 2, 3 } }));

            Assert.Equal("http://example.com:1234/path?key=0&value=zero#section", UriUtils.BuildUrl(new[] { "http://example.com:1234/", "path" }, new { key = 0, value = "zero" }, "section"));
            Assert.Equal("http://example.com:1234/path?value=1&value=2&value=3#section", UriUtils.BuildUrl(new[] { "http://example.com:1234/", "path" }, new Dictionary<string, object> { ["value"] = new[] { 1, 2, 3 } }, "section"));

            Assert.Equal("http://example.com:1234/path?key=0&value=zero#s%23ct%3Fon", UriUtils.BuildUrl(new[] { "http://example.com:1234/", "path" }, new { key = 0, value = "zero" }, "s#ct?on"));
        }

        [Fact]
        public void GetCanonicalPathTest()
        {
            Assert.Same("a/b/c", UriUtils.GetCanonicalPath("a/b/c"));

            Assert.Equal("a/c", UriUtils.GetCanonicalPath("a/b/.././/c"));
            Assert.Equal("/a/c", UriUtils.GetCanonicalPath("/a/b/.././/c"));

            Assert.Equal("../c", UriUtils.GetCanonicalPath("a/../../c"));
            Assert.Equal("/../c", UriUtils.GetCanonicalPath("/a/../../c"));
        }
    }
}
