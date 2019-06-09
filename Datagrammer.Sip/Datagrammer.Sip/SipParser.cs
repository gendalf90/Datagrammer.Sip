using Microsoft.Extensions.Primitives;
using System;
using System.Text;

namespace Datagrammer.Sip
{
    public static class SipParser
    {
        private const byte SpaceByte = (byte)' ';
        private const int StatusCodeLenght = 3;
        private const string CRLFString = "\r\n";

        private static readonly byte[] CRLFBytes = Encoding.UTF8.GetBytes("\r\n");
        private static readonly byte[] VersionBytes = Encoding.UTF8.GetBytes("SIP/2.0");
        private static readonly byte[] EndOfHeaders = Encoding.UTF8.GetBytes("\r\n\r\n");
        private static readonly char[] WordDelimiters = new[] { ' ' };

        public static bool TryParseRequest(ReadOnlyMemory<byte> bytes, out SipRequest request)
        {
            request = new SipRequest();

            if (!TrySliceHeaders(bytes, out var headers))
            {
                return false;
            }

            if (!TrySliceLine(headers, out var firstLine))
            {
                return false;
            }

            var method = SliceWord(firstLine);

            if (!IsMethodValid(method))
            {
                return false;
            }

            if(!TryToSkipWordWithSpace(firstLine, out var lineAfterMethod))
            {
                return false;
            }

            var uri = SliceWord(lineAfterMethod);

            if(uri.IsEmpty)
            {
                return false;
            }

            if (!TryToSkipWordWithSpace(lineAfterMethod, out var lineAfterUri))
            {
                return false;
            }

            var version = SliceWord(lineAfterUri);

            if (!IsVersionValid(version))
            {
                return false;
            }

            var lineAfterVersion = SkipWord(lineAfterUri);

            if (!lineAfterVersion.IsEmpty)
            {
                return false;
            }

            var parsedHeaders = ToUTF8String(headers.Span);
            var bodyBytes = bytes.Slice(headers.Length);

            request = CreateRequest(parsedHeaders, bodyBytes);
            return true;
        }

        public static bool TryParseResponse(ReadOnlyMemory<byte> bytes, out SipResponse response)
        {
            response = new SipResponse();

            if(!TrySliceHeaders(bytes, out var headers))
            {
                return false;
            }

            if (!TrySliceLine(headers, out var firstLine))
            {
                return false;
            }

            var version = SliceWord(firstLine);

            if (!IsVersionValid(version))
            {
                return false;
            }

            if(!TryToSkipWordWithSpace(firstLine, out var lineAfterVersion))
            {
                return false;
            }

            var statusCode = SliceWord(lineAfterVersion);

            if(!IsStatusCodeValid(statusCode))
            {
                return false;
            }

            if (!TryToSkipWordWithSpace(lineAfterVersion, out var lineAfterStatusCode))
            {
                return false;
            }

            var lineAfterReasonPhrase = SkipWord(lineAfterStatusCode);

            if(!lineAfterReasonPhrase.IsEmpty)
            {
                return false;
            }

            var parsedHeaders = ToUTF8String(headers.Span);
            var bodyBytes = bytes.Slice(headers.Length);

            response = CreateResponse(parsedHeaders, bodyBytes);
            return true;
        }

        private static string ToUTF8String(ReadOnlySpan<byte> bytes)
        {
            unsafe
            {
                fixed (byte* bytesPointer = bytes)
                {
                    return Encoding.UTF8.GetString(bytesPointer, bytes.Length);
                }
            }
        }

        private static SipRequest CreateRequest(StringSegment headers, ReadOnlyMemory<byte> body)
        {
            var words = GetFirstLineWords(headers).GetEnumerator();

            words.MoveNext();
            var method = words.Current;
            words.MoveNext();
            var uri = words.Current;
            words.MoveNext();
            var version = words.Current;

            return new SipRequest(headers, method, uri, version, body);
        }

        private static SipResponse CreateResponse(StringSegment headers, ReadOnlyMemory<byte> body)
        {
            var words = GetFirstLineWords(headers).GetEnumerator();

            words.MoveNext();
            var version = words.Current;
            words.MoveNext();
            var statusCode = words.Current;
            words.MoveNext();
            var reasonPhrase = words.Current;

            return new SipResponse(headers, statusCode, reasonPhrase, version, body);
        }

        private static StringTokenizer GetFirstLineWords(StringSegment headers)
        {
            var lineBreakIndex = headers.AsSpan().IndexOf(CRLFString.AsSpan());
            var firstLine = headers.Subsegment(0, lineBreakIndex);
            return firstLine.Split(WordDelimiters);
        }

        private static bool TrySliceHeaders(ReadOnlyMemory<byte> bytes, out ReadOnlyMemory<byte> headers)
        {
            headers = ReadOnlyMemory<byte>.Empty;

            var messageBreakIndex = bytes.Span.IndexOf(EndOfHeaders.AsSpan());

            if(messageBreakIndex < 0)
            {
                return false;
            }

            headers = bytes.Slice(0, messageBreakIndex + EndOfHeaders.Length);
            return true;
        }

        private static bool TrySliceLine(ReadOnlyMemory<byte> bytes, out ReadOnlyMemory<byte> line)
        {
            line = ReadOnlyMemory<byte>.Empty;

            var lineBreakIndex = bytes.Span.IndexOf(CRLFBytes.AsSpan());

            if (lineBreakIndex < 0)
            {
                return false;
            }

            line = bytes.Slice(0, lineBreakIndex);
            return true;
        }

        private static ReadOnlyMemory<byte> SliceWord(ReadOnlyMemory<byte> bytes)
        {
            var spaceIndex = bytes.Span.IndexOf(SpaceByte);

            if (spaceIndex < 0)
            {
                return bytes;
            }

            return bytes.Slice(0, spaceIndex);
        }

        private static ReadOnlyMemory<byte> SkipWord(ReadOnlyMemory<byte> bytes)
        {
            var spaceIndex = bytes.Span.IndexOf(SpaceByte);

            if (spaceIndex < 0)
            {
                return ReadOnlyMemory<byte>.Empty;
            }

            return bytes.Slice(spaceIndex);
        }

        private static bool TryToSkipWordWithSpace(ReadOnlyMemory<byte> bytes, out ReadOnlyMemory<byte> result)
        {
            result = ReadOnlyMemory<byte>.Empty;

            var remainsAfterWord = SkipWord(bytes);

            if(remainsAfterWord.IsEmpty)
            {
                return false;
            }

            if(remainsAfterWord.Span[0] != SpaceByte)
            {
                return false;
            }

            result = remainsAfterWord.Slice(1);
            return true;
        }

        private static bool IsVersionValid(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Span.SequenceEqual(VersionBytes.AsSpan());
        }

        private static bool IsMethodValid(ReadOnlyMemory<byte> bytes)
        {
            return !bytes.IsEmpty && SipCharacters.HasValidTokenChars(bytes.Span);
        }

        private static bool IsStatusCodeValid(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Length == StatusCodeLenght && SipCharacters.HasOnlyDigits(bytes.Span);
        }
    }
}
