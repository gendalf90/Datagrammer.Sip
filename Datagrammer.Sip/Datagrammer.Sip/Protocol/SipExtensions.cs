using Microsoft.Extensions.Primitives;

namespace Sip.Protocol
{
    public static class SipExtensions
    {
        public static SipHeaderRowValues SplitValue(this SipHeader header)
        {
            return new SipHeaderRowValues(header.Value);
        }

        public static bool IsSingleName(this SipParameter parameter)
        {
            return parameter.Value == StringSegment.Empty;
        }
    }
}
