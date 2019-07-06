using Sip.Protocol;
using Xunit;

namespace Tests.Unit
{
    public class ViaHeaderTests
    {
        [Fact]
        public void ParseFullHeader()
        {
            var header = "SIP/2.0/UDP  server10.biloxi.com  ; branch=  z9hG4bKnashds8 ; received =192.0.2.3;test";

            var result = ViaHeader.TryParse(header, out var parsed);
            var parameters = parsed.Parameters.GetEnumerator();

            Assert.True(result);
            Assert.Equal("SIP/2.0", parsed.Version);
            Assert.Equal("UDP", parsed.Protocol);
            Assert.Equal("server10.biloxi.com", parsed.Host);
            Assert.True(parameters.MoveNext());
            Assert.Equal("branch", parameters.Current.Name);
            Assert.Equal("z9hG4bKnashds8", parameters.Current.Value);
            Assert.True(parameters.MoveNext());
            Assert.Equal("received", parameters.Current.Name);
            Assert.Equal("192.0.2.3", parameters.Current.Value);
            Assert.True(parameters.MoveNext());
            Assert.Equal("test", parameters.Current.Name);
        }

        [Theory]
        [InlineData("SIP/3.0/UDP server10.biloxi.com")]
        [InlineData("SIP/2.0/UDP server10@biloxi.com")]
        [InlineData("SIP/2.0/@UDP] server10.biloxi.com")]
        [InlineData("SIP/2.0/UDP")]
        [InlineData("SIP/2.0/UDP      ")]
        [InlineData("SIP/2.0/UDP ;test: asdf")]
        [InlineData("server10.biloxi.com")]
        [InlineData("SIP/2.0/UDP asdf server10.biloxi.com")]
        [InlineData("UDP server10.biloxi.com")]
        [InlineData("SIP/2.0 server10.biloxi.com")]
        [InlineData("SIP/2.0/     server10.biloxi.com")]
        public void ParseHeader_HeaderIsInvalid(string header)
        {
            var result = ViaHeader.TryParse(header, out var parsed);

            Assert.False(result);
        }

        [Theory]
        [InlineData("SIP/2.0/UDP server10.biloxi.com")]
        [InlineData("SIP/2.0/+*5 server10.biloxi.com")]
        [InlineData("SIP/2.0/UDP server")]
        [InlineData("SIP/2.0/UDP server  ;")]
        public void ParseHeader_HeaderIsValid(string header)
        {
            var result = ViaHeader.TryParse(header, out var parsed);

            Assert.True(result);
        }
    }
}
