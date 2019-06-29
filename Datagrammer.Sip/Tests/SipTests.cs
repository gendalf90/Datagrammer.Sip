using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;
using oSIP.Net;
using System.Net.Http.Headers;
using System.Text;
using Xunit;

namespace Tests
{
    public class SipTests
    {
        [Fact]
        public void Test1()
        {
            var str = @"INVITE sip:asdf@biloxi.com SIP/2.0
Via: SIP/2.0/UDP pc3_3^.atlanta.com; branch = z9hG4bKkjshdyfferdsdwdf
To: Bob <sip:bob@biloxi.com>
From: Alice of death <sip:alice@atlanta.com>;tag = 88sja8x;
Max-Forwards: 70
Call-ID: 987asjd97y7atg
CSeq: 986759 INVITE
Route: Bob <sip:bob@biloxi.com>, Gregg <sip:gregg@biloxi.com>
";

            //var i = str.IndexOf("z9hG4bKkjshdyff");
            //str = str.Insert(i, "\"").Insert(i + 6, ";").Insert(i + 9, "\"");

            var bytes = Encoding.UTF8.GetBytes(str);

            var value = (SipRequest)SipMessage.Parse(str);

            //var value = SIP_Request.Parse(bytes);

        }
    }
}
