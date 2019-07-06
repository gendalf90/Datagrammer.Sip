using Sip.Protocol;
using Xunit;

namespace Tests.Unit
{
    public class ParameterTests
    {
        [Fact]
        public void ParseFullParameters()
        {
            var parameters = new SipParameters(";^te%st1=|valu~e1;  test2 =  value2   ;test3   =\"значение(3)\";test4 ;test5=value5    ").GetEnumerator();

            Assert.True(parameters.MoveNext());
            Assert.Equal("^te%st1", parameters.Current.Name);
            Assert.Equal("|valu~e1", parameters.Current.Value);
            Assert.True(parameters.MoveNext());
            Assert.Equal("test2", parameters.Current.Name);
            Assert.Equal("value2", parameters.Current.Value);
            Assert.True(parameters.MoveNext());
            Assert.Equal("test3", parameters.Current.Name);
            Assert.Equal("\"значение(3)\"", parameters.Current.Value);
            Assert.True(parameters.MoveNext());
            Assert.Equal("test4", parameters.Current.Name);
            Assert.True(parameters.MoveNext());
            Assert.Equal("test5", parameters.Current.Name);
            Assert.Equal("value5", parameters.Current.Value);
        }

        [Theory]
        [InlineData("key=value", 0, null, null)]
        [InlineData(";k\"e\"y", 0, null, null)]
        [InlineData(";key1;key2;key3", 3, new[] { "key1", "key2", "key3" }, new[] { "", "", "" })]
        [InlineData(";key1=\"value1;key2=value2", 1, new[] { "key2" }, new[] { "value2" })]
        [InlineData(";key1=asdf\"value1\";key2=value2", 1, new[] { "key2" }, new[] { "value2" })]
        [InlineData(";key1=asdf\"value1\"qwer;key2=value2", 1, new[] { "key2" }, new[] { "value2" })]
        [InlineData(";key1=\"value1\"qwer;key2=value2", 1, new[] { "key2" }, new[] { "value2" })]
        [InlineData(";key1=;key2=value2", 1, new[] { "key2" }, new[] { "value2" })]
        [InlineData(";k[e]y1=value1;key2=value2", 1, new[] { "key2" }, new[] { "value2" })]
        [InlineData(";key1=value1;key2=va(l)ue2", 1, new[] { "key1" }, new[] { "value1" })]
        [InlineData(";key1=\"va\\\"lue1\"", 1, new[] { "key1" }, new[] { "\"va\\\"lue1\"" })]
        [InlineData(";key1=\"va\"lue1\";key2=value2", 1, new[] { "key2" }, new[] { "value2" })]
        [InlineData(";key1=\"v a l u e 1\"", 1, new[] { "key1" }, new[] { "\"v a l u e 1\"" })]
        [InlineData(";key1=\"value1;value2\"", 1, new[] { "key1" }, new[] { "\"value1;value2\"" })]
        [InlineData(";key1=   value1  ;", 1, new[] { "key1" }, new[] { "value1" })]
        [InlineData(";key1=va   lue1;key2=value2", 1, new[] { "key2" }, new[] { "value2" })]
        public void ParseParameters_ParametersAreExpected(string value, int count, string[] names, string[] values)
        {
            var parameters = new SipParameters(value).GetEnumerator();

            for(int i = 0; i < count; i++)
            {
                Assert.True(parameters.MoveNext());
                Assert.Equal(names[i], parameters.Current.Name);
                Assert.Equal(values[i], parameters.Current.Value);
            }

            Assert.False(parameters.MoveNext());
        }
    }
}
