using Microsoft.Extensions.Primitives;
using System;

namespace Sip.Protocol
{
    public readonly struct ViaHeader
    {
        private const char VersionAndProtocolSeparator = '/';
        private const char ParameterSeparatorChar = ';';
        private const string VersionString = "SIP/2.0";

        internal ViaHeader(StringSegment version,
                           StringSegment protocol,
                           StringSegment host,
                           SipParameters parameters)
        {
            Version = version;
            Protocol = protocol;
            Host = host;
            Parameters = parameters;
        }

        public StringSegment Version { get; }

        public StringSegment Protocol { get; }

        public StringSegment Host { get; }

        public SipParameters Parameters { get; }

        public static bool TryParse(StringSegment chars, out ViaHeader header)
        {
            header = new ViaHeader();
            
            if(!TrySliceVersion(chars, out var afterVersionChars))
            {
                return false;
            }

            if(!TrySliceFirstCharacter(afterVersionChars, VersionAndProtocolSeparator, out var protocolChars))
            {
                return false;
            }

            var protocol = ReadProtocol(protocolChars);

            if(!IsProtocolValid(protocol))
            {
                return false;
            }

            var afterProtocolChars = protocolChars.Subsegment(protocol.Length).TrimStart();
            var host = ReadHost(afterProtocolChars);

            if(!IsHostValid(host))
            {
                return false;
            }

            var afterHostChars = afterProtocolChars.Subsegment(host.Length).Trim();

            if(!IsParametersOrEmpty(afterHostChars))
            {
                return false;
            }

            header = new ViaHeader(VersionString,
                                   protocol,
                                   host,
                                   new SipParameters(afterHostChars));
            return true;
        }

        private static bool TrySliceVersion(StringSegment chars, out StringSegment remainingChars)
        {
            remainingChars = StringSegment.Empty;

            if (!chars.AsSpan().StartsWith(VersionString.AsSpan()))
            {
                return false;
            }

            remainingChars = chars.Subsegment(VersionString.Length);
            return true;
        }

        private static bool TrySliceFirstCharacter(StringSegment chars, char first, out StringSegment remainingChars)
        {
            remainingChars = StringSegment.Empty;

            if(chars == StringSegment.Empty)
            {
                return false;
            }

            if (chars[0] != first)
            {
                return false;
            }

            remainingChars = chars.Subsegment(1);
            return true;
        }

        private static StringSegment ReadProtocol(StringSegment chars)
        {
            var spaceIndex = SipCharacters.IndexOfWhitespace(chars);

            if(spaceIndex == -1)
            {
                return chars;
            }

            return chars.Subsegment(0, spaceIndex);
        }

        private static StringSegment ReadHost(StringSegment chars)
        {
            var separatorIndex = IndexOfAnySeparator(chars);

            if (separatorIndex == -1)
            {
                return chars;
            }

            return chars.Subsegment(0, separatorIndex);
        }

        private static int IndexOfAnySeparator(StringSegment chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsWhiteSpace(chars[i]) || chars[i] == ParameterSeparatorChar)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsProtocolValid(StringSegment protocol)
        {
            if(protocol == StringSegment.Empty)
            {
                return false;
            }

            if(!SipCharacters.IsValidToken(protocol))
            {
                return false;
            }

            return true;
        }

        private static bool IsHostValid(StringSegment host)
        {
            return host != StringSegment.Empty && SipCharacters.IsValidHost(host);
        }

        private static bool IsParametersOrEmpty(StringSegment chars)
        {
            if(chars == StringSegment.Empty)
            {
                return true;
            }

            if(chars[0] == ParameterSeparatorChar)
            {
                return true;
            }

            return false;
        }
    }
}
