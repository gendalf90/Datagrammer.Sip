using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sip.Protocol
{
    public readonly struct SipBuilderStep
    {
        private const string Version = "SIP/2.0";
        private const string CRLF = "\r\n";
        private const int StatusCodeLenght = 3;
        private const int FirstLineAdditionalLength = 11;
        private const int HeaderAdditionalLength = 3;
        private const char WordDelimiter = ' ';
        private const char HeaderNameValueDelimiter = ':';

        private static readonly byte[] CRLFBytes = Encoding.UTF8.GetBytes("\r\n");

        private readonly int firstLineLength;
        private readonly ReadOnlyMemory<char> header;
        private readonly ReadOnlyMemory<byte> body;

        private SipBuilderStep(ReadOnlyMemory<char> header, ReadOnlyMemory<byte> body, int firstLineLength)
        {
            this.header = header;
            this.body = body;
            this.firstLineLength = firstLineLength;
        }

        public SipBuilderStep SetRequestHeader(StringSegment method, StringSegment uri)
        {
            if (!IsMethodValid(method))
            {
                throw new ArgumentException("Is not valid", nameof(method));
            }

            if (!IsUriValid(uri))
            {
                throw new ArgumentException("Is not valid", nameof(uri));
            }

            var headerWithoutFirstLine = header.Slice(firstLineLength);
            var newFirstLineLength = method.Length + uri.Length + FirstLineAdditionalLength;
            var newHeader = new char[headerWithoutFirstLine.Length + newFirstLineLength];
            var remainsOfNewHeader = newHeader.AsSpan();

            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, method.AsSpan());
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, WordDelimiter);
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, uri.AsSpan());
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, WordDelimiter);
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, Version.AsSpan());
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, CRLF.AsSpan());
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, headerWithoutFirstLine.Span);

            return new SipBuilderStep(newHeader, body, newFirstLineLength);
        }

        public SipBuilderStep SetResponseHeader(StringSegment statusCode, StringSegment reasonPhrase)
        {
            if(!IsStatusCodeValid(statusCode))
            {
                throw new ArgumentException("Is not valid", nameof(statusCode));
            }

            if (!IsReasonPhraseValid(reasonPhrase))
            {
                throw new ArgumentException("Is not valid", nameof(reasonPhrase));
            }

            var headerWithoutFirstLine = header.Slice(firstLineLength);
            var newFirstLineLength = statusCode.Length + reasonPhrase.Length + FirstLineAdditionalLength;
            var newHeader = new char[headerWithoutFirstLine.Length + newFirstLineLength];
            var remainsOfNewHeader = newHeader.AsSpan();

            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, Version.AsSpan());
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, WordDelimiter);
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, statusCode.AsSpan());
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, WordDelimiter);
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, reasonPhrase.AsSpan());
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, CRLF.AsSpan());
            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, headerWithoutFirstLine.Span);

            return new SipBuilderStep(newHeader, body, newFirstLineLength);
        }

        public SipBuilderStep AddHeaders(KeyValuePair<StringSegment, StringSegment>[] headers)
        {
            if(headers.Length == 0)
            {
                return new SipBuilderStep(header, body, firstLineLength);
            }

            for(int i = 0; i < headers.Length; i++)
            {
                ValidateHeader(headers[i].Key, headers[i].Value);
            }

            var newHeaderLength = header.Length;

            for (int i = 0; i < headers.Length; i++)
            {
                newHeaderLength += headers[i].Key.Length + headers[i].Value.Length + HeaderAdditionalLength;
            }

            var newHeader = new char[newHeaderLength];
            var remainsOfNewHeader = newHeader.AsSpan();

            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, header.Span);

            for (int i = 0; i < headers.Length; i++)
            {
                remainsOfNewHeader = WriteAndSliceHeader(remainsOfNewHeader, headers[i].Key, headers[i].Value);
            }

            return new SipBuilderStep(newHeader, body, firstLineLength);
        }

        public SipBuilderStep AddHeader(StringSegment name, StringSegment value)
        {
            ValidateHeader(name, value);

            var currentHeaderLength = name.Length + value.Length + HeaderAdditionalLength;
            var newHeader = new char[header.Length + currentHeaderLength];
            var remainsOfNewHeader = newHeader.AsSpan();

            remainsOfNewHeader = WriteAndSlice(remainsOfNewHeader, header.Span);
            remainsOfNewHeader = WriteAndSliceHeader(remainsOfNewHeader, name, value);
            
            return new SipBuilderStep(newHeader, body, firstLineLength);
        }

        private void ValidateHeader(StringSegment headerName, StringSegment headerValue)
        {
            if (!IsHeaderNameValid(headerName))
            {
                throw new ArgumentException("Is not valid", nameof(headerName));
            }

            if (!IsHeaderValueValid(headerValue))
            {
                throw new ArgumentException("Is not valid", nameof(headerValue));
            }
        }

        private Span<char> WriteAndSliceHeader(Span<char> destination, StringSegment headerName, StringSegment headerValue)
        {
            var remainsOfDestination = destination;
            remainsOfDestination = WriteAndSlice(remainsOfDestination, headerName.AsSpan());
            remainsOfDestination = WriteAndSlice(remainsOfDestination, HeaderNameValueDelimiter);
            remainsOfDestination = WriteAndSlice(remainsOfDestination, headerValue.AsSpan());
            return WriteAndSlice(remainsOfDestination, CRLF.AsSpan());
        }

        public SipBuilderStep SetBody(ReadOnlyMemory<byte> bytes)
        {
            return new SipBuilderStep(header, bytes, firstLineLength);
        }

        public ReadOnlyMemory<byte> Build()
        {
            if(firstLineLength == 0)
            {
                throw new ArgumentException("Is empty", "FirstLine");
            }

            var headerByteLength = GetUTF8ByteLength(header.Span);
            var messageByteLength = headerByteLength + CRLFBytes.Length + body.Length;
            var messageBytes = new byte[messageByteLength];
            var remainsOfMessageBytes = messageBytes.AsSpan();

            remainsOfMessageBytes = WriteAndSliceUTF8Chars(remainsOfMessageBytes, header.Span);
            remainsOfMessageBytes = WriteAndSlice(remainsOfMessageBytes, CRLFBytes.AsSpan());
            remainsOfMessageBytes = WriteAndSlice(remainsOfMessageBytes, body.Span);

            return messageBytes;
        }

        private int GetUTF8ByteLength(ReadOnlySpan<char> chars)
        {
            unsafe
            {
                fixed (char* charsPointer = chars)
                {
                    return Encoding.UTF8.GetByteCount(charsPointer, header.Length);
                }
            }
        }

        private Span<byte> WriteAndSliceUTF8Chars(Span<byte> destination, ReadOnlySpan<char> chars)
        {
            unsafe
            {
                fixed (byte* bytesPointer = destination)
                fixed (char* charsPointer = chars)
                {
                    var writtenByteCount = Encoding.UTF8.GetBytes(charsPointer, chars.Length, bytesPointer, destination.Length);
                    return destination.Slice(writtenByteCount);
                }
            }
        }

        private Span<T> WriteAndSlice<T>(Span<T> destination, ReadOnlySpan<T> value)
        {
            value.CopyTo(destination);
            return destination.Slice(value.Length);
        }

        private Span<char> WriteAndSlice(Span<char> destination, char value)
        {
            destination[0] = value;
            return destination.Slice(1);
        }

        private static bool IsMethodValid(ReadOnlyMemory<char> chars)
        {
            return !chars.IsEmpty && SipCharacters.IsValidToken(chars.Span);
        }

        private static bool IsUriValid(ReadOnlyMemory<char> chars)
        {
            return !chars.IsEmpty && SipCharacters.IsValidUri(chars.Span);
        }

        private static bool IsStatusCodeValid(ReadOnlyMemory<char> chars)
        {
            return chars.Length == StatusCodeLenght && SipCharacters.IsDigits(chars.Span);
        }

        private static bool IsReasonPhraseValid(ReadOnlyMemory<char> chars)
        {
            return !SipCharacters.HasCROrLF(chars.Span);
        }

        private static bool IsHeaderNameValid(ReadOnlyMemory<char> chars)
        {
            return !chars.IsEmpty && SipCharacters.IsValidField(chars.Span);
        }

        private static bool IsHeaderValueValid(ReadOnlyMemory<char> chars)
        {
            return !chars.IsEmpty;
        }
    }
}
