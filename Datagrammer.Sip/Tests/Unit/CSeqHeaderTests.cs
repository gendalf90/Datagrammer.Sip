using Sip.Protocol;
using Xunit;

namespace Tests.Unit
{
    public class CSeqHeaderTests
    {
        [Theory]
        [InlineData("99 ACK", true, "ACK", "99")]
        [InlineData("1", false, null, null)]
        [InlineData("ACK", false, null, null)]
        [InlineData("99      ACK", true, "ACK", "99")]
        [InlineData("  99  ACK      ", true, "ACK", "99")]
        [InlineData("99a ACK", false, null, null)]
        [InlineData("99 (ACK);", false, null, null)]
        [InlineData("99 qwer asdf", false, null, null)]
        [InlineData("99 +A~CK|", true, "+A~CK|", "99")]
        public void ParseValues(string data, bool isParsed, string method, string number)
        {
            var result = CSeqHeader.TryParse(data, out var header);

            Assert.Equal(isParsed, result);
            Assert.Equal(method, header.Method);
            Assert.Equal(number, header.SequenceNumber);
        }
    }
}
