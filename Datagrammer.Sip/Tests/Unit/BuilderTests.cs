using Microsoft.Extensions.Primitives;
using Sip.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Tests.Unit
{
    public class BuilderTests
    {
        [Fact]
        public void BuildRequest()
        {
            var result = new SipBuilderStep().SetRequestHeader("REGISTER", "sip:ss2.wcom.com")
                                             .AddHeader("Via", "SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh")
                                             .AddHeader("From", "LittleGuy <sip:UserB@there.com>")
                                             .AddHeader("Content-Length", "8")
                                             .SetBody(new byte[] { 1, 2, 3 })
                                             .Build()
                                             .ToArray();

            var expectedMessage = @"REGISTER sip:ss2.wcom.com SIP/2.0
Via:SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh
From:LittleGuy <sip:UserB@there.com>
Content-Length:8

";
            var expectedBytes = Encoding.UTF8.GetBytes(expectedMessage)
                                             .Concat(new byte[] { 1, 2, 3 })
                                             .ToArray();

            Assert.True(result.SequenceEqual(expectedBytes));
        }

        [Fact]
        public void BuildResponse()
        {
            var result = new SipBuilderStep().AddHeader("Call-ID", "123456789@there.com")
                                             .SetResponseHeader("200", "IT'S OK, BRO!")
                                             .Build()
                                             .ToArray();

            var expectedMessage = @"SIP/2.0 200 IT'S OK, BRO!
Call-ID:123456789@there.com

";
            var expectedBytes = Encoding.UTF8.GetBytes(expectedMessage);

            Assert.True(result.SequenceEqual(expectedBytes));
        }

        [Fact]
        public void BuildRequest_AddHeaderList()
        {
            var headers = new KeyValuePair<StringSegment, StringSegment>[]
            {
                new KeyValuePair<StringSegment, StringSegment>("Via", "SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh"),
                new KeyValuePair<StringSegment, StringSegment>("Content-Length", "8")
            };

            var result = new SipBuilderStep().SetRequestHeader("REGISTER", "sip:ss2.wcom.com")
                                             .AddHeaders(headers)
                                             .Build()
                                             .ToArray();

            var expectedMessage = @"REGISTER sip:ss2.wcom.com SIP/2.0
Via:SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh
Content-Length:8

";
            var expectedBytes = Encoding.UTF8.GetBytes(expectedMessage);

            Assert.True(result.SequenceEqual(expectedBytes));
        }
    }
}
