using Sip.Protocol;
using Xunit;

namespace Tests.Unit
{
    public class HeaderRowValuesTests
    {
        [Theory]
        [InlineData("asdf,qwer", new[] { "asdf", "qwer" })]
        [InlineData("asdf", new[] { "asdf" })]
        [InlineData("   asdf  ,    qwer  ", new[] { "asdf", "qwer" })]
        [InlineData("asdf,qwer,", new[] { "asdf", "qwer" })]
        [InlineData("asdf,qwer,     ", new[] { "asdf", "qwer" })]
        [InlineData("\"as,df\", qwer", new[] { "\"as,df\"", "qwer" })]
        [InlineData("<as,df>, qwer", new[] { "<as,df>", "qwer" })]
        [InlineData("\"asdf<\" , \">qwer\"", new[] { "\"asdf<\"", "\">qwer\"" })]
        [InlineData("asdf\",<q,w>er", new[] { "asdf\"", "<q,w>er" })]
        [InlineData("\"a<sd>,f\",qwer", new[] { "\"a<sd>,f\"", "qwer" })]
        [InlineData("a<s\"d>f,<qw\"er>", new[] { "a<s\"d>f", "<qw\"er>" })]
        [InlineData("asdf<,\">qwer", new[] { "asdf<,\">qwer" })]
        [InlineData("asdf<,\">qw\"er", new[] { "asdf<,\">qw\"er" })]
        [InlineData("\"test\\\" , \\\"test\" <test,test>;test=\"<test,test>\", \"test < 0\" <test,test>;test=\"test>0\"", new[] 
        {
            "\"test\\\" , \\\"test\" <test,test>;test=\"<test,test>\"",
            "\"test < 0\" <test,test>;test=\"test>0\""
        })]
        [InlineData("\"as\"df, qwer\" ,zxcv", new[] { "\"as\"df", "qwer\"", "zxcv" })]
        [InlineData("\"as\\\"df, qwer\" ,zxcv", new[] { "\"as\\\"df, qwer\"", "zxcv" })]
        public void ParseValues(string data, string[] parsedValues)
        {
            var values = new SipHeaderRowValues(data).GetEnumerator();

            foreach(var parsedValue in parsedValues)
            {
                Assert.True(values.MoveNext());
                Assert.Equal(parsedValue, values.Current);
            }

            Assert.False(values.MoveNext());
        }
    }
}
