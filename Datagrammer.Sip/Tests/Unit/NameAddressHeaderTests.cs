using Sip.Protocol;
using Xunit;

namespace Tests.Unit
{
    public class NameAddressHeaderTests
    {
        [Fact]
        public void ParseFullHeader()
        {
            var header = "Alice from Atlanta  <sip:alice@atlanta.com> ;  tag =  1928301774";

            var result = NameAddressHeader.TryParse(header, out var parsed);
            var parameters = parsed.Parameters.GetEnumerator();

            Assert.True(result);
            Assert.Equal("Alice from Atlanta", parsed.DisplayName);
            Assert.Equal("<sip:alice@atlanta.com>", parsed.Uri);
            Assert.True(parameters.MoveNext());
            Assert.Equal("tag", parameters.Current.Name);
            Assert.Equal("1928301774", parameters.Current.Value);
        }

        [Theory]
        [InlineData("Alice from Atlanta <sip:alice@atlanta.com>", "Alice from Atlanta", "<sip:alice@atlanta.com>")]
        [InlineData("Alice    <sip:alice@atlanta.com>", "Alice", "<sip:alice@atlanta.com>")]
        [InlineData("<sip:alice@atlanta.com>", "", "<sip:alice@atlanta.com>")]
        [InlineData("sip:alice@atlanta.com", "", "sip:alice@atlanta.com")]
        [InlineData("Alice<sip:alice@atlanta.com>", "Alice", "<sip:alice@atlanta.com>")]
        [InlineData("\"кто-то\" <sip:alice@atlanta.com>", "\"кто-то\"", "<sip:alice@atlanta.com>")]
        [InlineData("\"кто-\\\"то\\\"\" <sip:alice@atlanta.com>", "\"кто-\\\"то\\\"\"", "<sip:alice@atlanta.com>")]
        public void ParseValidHeader_DisplayNameAndUriAreExpected(string header, string displayName, string uri)
        {
            var result = NameAddressHeader.TryParse(header, out var parsed);

            Assert.True(result);
            Assert.Equal(displayName, parsed.DisplayName);
            Assert.Equal(uri, parsed.Uri);
        }

        [Theory]
        [InlineData("кто-то <sip:alice@atlanta.com>")]
        [InlineData("Alice <sip:алиса@atlanta.com>")]
        [InlineData("Alice \"From\" Atlanta <sip:alice@atlanta.com>")]
        [InlineData("кто-то <sip:alice@atlanta.com> asdf")]
        [InlineData("кто-то <sip:alice@atlanta.com> asdf;")]
        [InlineData("Alice sip:alice@atlanta.com")]
        [InlineData("\"Alice <sip:alice@atlanta.com>")]
        public void ParseHeader_HeaderIsInvalid(string header)
        {
            var result = NameAddressHeader.TryParse(header, out var parsed);

            Assert.False(result);
        }

        [Theory]
        [InlineData("Alice <sip:alice@atlanta.com>")]
        [InlineData("Alice From Atlanta <sip:alice@atlanta.com>  ")]
        [InlineData("\"Алиса из Атланты\" <sip:alice@atlanta.com>")]
        [InlineData("sip:alice@atlanta.com")]
        [InlineData("<sip:alice@atlanta.com>")]
        public void ParseHeader_HeaderIsValid(string header)
        {
            var result = NameAddressHeader.TryParse(header, out var parsed);

            Assert.True(result);
        }
    }
}
