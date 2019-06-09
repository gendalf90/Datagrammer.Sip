using Datagrammer.Sip;
using System;
using System.Text;
using Xunit;

namespace Tests
{
    public class SipTests
    {
        [Fact]
        public void Test1()
        {
            var str = @"REGISTER sip:8355@ideasip.com SIP/2.0
";
//            var str = @"POST /resource/?query_id=0 HTTP/1.1
//Host: example.com
//User-Agent: custom
//Accept: */*
//Connection: close
//Content-Length: 20
//Content-Type: application/json

//{""key1"":1, ""key2"":2}";
            var bytes = Encoding.UTF8.GetBytes(str);
            //var parser = new SipRequestParser();

            //parser.Parse(bytes);

        }
    }
}
