using Sip.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Unit
{
    public class ParseTests
    {
        private static readonly string EndOfHeaders = $"{Environment.NewLine}{Environment.NewLine}";

        [Fact]
        public void ParseFullRequest()
        {
            var message = @"REGISTER sip:ss2.wcom.com SIP/2.0
Via: SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh     
From: LittleGuy <sip:UserB@there.com>
To: sip:UserB@there.com;branch=sduf9897s;
Call-ID : 123456789@there.com
CSeq:1 REGISTER
Content-Length: 8

asdf";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            var result = SipParser.TryParseRequest(messageBytes, out var request);
            var headers = new List<SipHeader>();

            foreach (var header in request.Headers)
            {
                headers.Add(header);
            }

            var body = Encoding.UTF8.GetString(request.Body.ToArray());

            Assert.True(result);
            Assert.Equal("REGISTER", request.Method);
            Assert.Equal("sip:ss2.wcom.com", request.Uri);
            Assert.Equal("SIP/2.0", request.Version);
            Assert.Equal(6, headers.Count);
            Assert.Equal("Via", headers[0].Name);
            Assert.Equal("SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh", headers[0].Value);
            Assert.Equal("From", headers[1].Name);
            Assert.Equal("LittleGuy <sip:UserB@there.com>", headers[1].Value);
            Assert.Equal("To", headers[2].Name);
            Assert.Equal("sip:UserB@there.com;branch=sduf9897s;", headers[2].Value);
            Assert.Equal("Call-ID", headers[3].Name);
            Assert.Equal("123456789@there.com", headers[3].Value);
            Assert.Equal("CSeq", headers[4].Name);
            Assert.Equal("1 REGISTER", headers[4].Value);
            Assert.Equal("Content-Length", headers[5].Name);
            Assert.Equal("8", headers[5].Value);
            Assert.Equal("asdf", body);
        }

        [Theory]
        [InlineData("регистрация sip:ss2.wcom.com SIP/2.0")]
        [InlineData("REGISTER какой-то@адрес.com SIP/2.0")]
        [InlineData("sip:ss2.wcom.com SIP/2.0")]
        [InlineData("REGISTER sip:ss2.wcom.com SIP/3.0")]
        [InlineData("REGISTER SIP/2.0")]
        [InlineData("REGISTER sip:ss2.wcom.com")]
        [InlineData("REGISTER")]
        public void ParseRequest_InvalidFirstLine_ResultIsFalse(string firstLine)
        {
            var message = $"{firstLine}{EndOfHeaders}";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            var result = SipParser.TryParseRequest(messageBytes, out var request);

            Assert.False(result);
        }

        [Theory]
        [InlineData("REGISTER sip:ss2.wcom.com SIP/2.0")]
        [InlineData("~reg_reg| sip:ss2.wcom.com SIP/2.0")]
        [InlineData("REGISTER asdsf SIP/2.0")]
        public void ParseRequest_ValidFirstLine_ResultIsTrue(string firstLine)
        {
            var message = $"{firstLine}{EndOfHeaders}";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            var result = SipParser.TryParseRequest(messageBytes, out var request);

            Assert.True(result);
        }

        [Fact]
        public void ParseFullResponse()
        {
            var message = @"SIP/2.0 200 IT'S OK, BRO! 
Via:SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh
From: LittleGuy <sip:UserB@there.com>
To: sip:UserB@there.com;branch=sduf9897s;
Call-ID: 123456789@there.com
CSeq   : 1 REGISTER
Contact: <sip:+1-972-555-2222@gw1.wcom.com;user=phone>;expires=3600    
Contact: <mailto:UserB@there.com>;expires=4294967295
Content-Length: 8  

asdf";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            var result = SipParser.TryParseResponse(messageBytes, out var response);
            var headers = new List<SipHeader>();

            foreach (var header in response.Headers)
            {
                headers.Add(header);
            }

            var body = Encoding.UTF8.GetString(response.Body.ToArray());

            Assert.True(result);
            Assert.Equal("SIP/2.0", response.Version);
            Assert.Equal("200", response.StatusCode);
            Assert.Equal("IT'S OK, BRO! ", response.ReasonPhrase);
            Assert.Equal(8, headers.Count);
            Assert.Equal("Via", headers[0].Name);
            Assert.Equal("SIP/2.0/UDP there.com:5060;branch=wsodil7987kjh", headers[0].Value);
            Assert.Equal("From", headers[1].Name);
            Assert.Equal("LittleGuy <sip:UserB@there.com>", headers[1].Value);
            Assert.Equal("To", headers[2].Name);
            Assert.Equal("sip:UserB@there.com;branch=sduf9897s;", headers[2].Value);
            Assert.Equal("Call-ID", headers[3].Name);
            Assert.Equal("123456789@there.com", headers[3].Value);
            Assert.Equal("CSeq", headers[4].Name);
            Assert.Equal("1 REGISTER", headers[4].Value);
            Assert.Equal("Contact", headers[5].Name);
            Assert.Equal("<sip:+1-972-555-2222@gw1.wcom.com;user=phone>;expires=3600", headers[5].Value);
            Assert.Equal("Contact", headers[6].Name);
            Assert.Equal("<mailto:UserB@there.com>;expires=4294967295", headers[6].Value);
            Assert.Equal("Content-Length", headers[7].Name);
            Assert.Equal("8", headers[7].Value);
            Assert.Equal("asdf", body);
        }

        [Theory]
        [InlineData("SIP/3.0 200 OK")]
        [InlineData("SIP/2.0 20b OK")]
        [InlineData("200 OK")]
        [InlineData("SIP/2.0 OK")]
        [InlineData("SIP/2.0")]
        [InlineData("SIP/2.0 2 OK")]
        [InlineData("SIP/2.0 200")]
        public void ParseResponse_InvalidFirstLine_ResultIsFalse(string firstLine)
        {
            var message = $"{firstLine}{EndOfHeaders}";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            var result = SipParser.TryParseResponse(messageBytes, out var response);

            Assert.False(result);
        }

        [Theory]
        [InlineData("SIP/2.0 200 OK")]
        [InlineData("SIP/2.0 500 какая-то причина (непонятная)")]
        [InlineData("SIP/2.0 200 ")]
        public void ParseResponse_ValidFirstLine_ResultIsTrue(string firstLine)
        {
            var message = $"{firstLine}{EndOfHeaders}";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            var result = SipParser.TryParseResponse(messageBytes, out var response);

            Assert.True(result);
        }

        [Theory]
        [InlineData("From: LittleGuy <sip:UserB@there.com>", "From", "LittleGuy <sip:UserB@there.com>")]
        [InlineData("[#some_name&1,1.1}: какое-то значение", "[#some_name&1,1.1}", "какое-то значение")]
        [InlineData("From   : LittleGuy <sip:UserB@there.com>", "From", "LittleGuy <sip:UserB@there.com>")]
        [InlineData("From:LittleGuy <sip:UserB@there.com>", "From", "LittleGuy <sip:UserB@there.com>")]
        [InlineData("From:     :Little;Guy <sip:UserB@there.com>", "From", ":Little;Guy <sip:UserB@there.com>")]
        public void ParseHeaders_HeaderIsValid(string header, string name, string value)
        {
            var message = $@"REGISTER sip:ss2.wcom.com SIP/2.0
{header}
{EndOfHeaders}";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            SipParser.TryParseRequest(messageBytes, out var request);
            var headerEnumerator = request.Headers.GetEnumerator();

            Assert.True(headerEnumerator.MoveNext());
            Assert.Equal(name, headerEnumerator.Current.Name);
            Assert.Equal(value, headerEnumerator.Current.Value);
        }

        [Theory]
        [InlineData("заголовок: LittleGuy <sip:UserB@there.com>")]
        [InlineData("From")]
        [InlineData("From:")]
        [InlineData("From:           ")]
        [InlineData("Fr om: asdf")]
        public void ParseHeaders_HeaderIsInvalid(string header)
        {
            var message = $@"REGISTER sip:ss2.wcom.com SIP/2.0
{header}
{EndOfHeaders}";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            SipParser.TryParseRequest(messageBytes, out var request);
            var headerEnumerator = request.Headers.GetEnumerator();

            Assert.False(headerEnumerator.MoveNext());
        }
    }
}
