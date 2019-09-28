using System;
using System.IO;

namespace plr
{
    public class ReadFullyStream : IcyMetaReaderStream
    {
        private readonly byte[] readAheadBuffer;
        private int readAheadLength;
        private int readAheadOffset;

        public ReadFullyStream(Stream sourceStream, int icyMetaInt) : base(sourceStream, icyMetaInt)
        {
            readAheadBuffer = new byte[4096];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = 0;
            while (bytesRead < count)
            {
                var readAheadAvailableBytes = readAheadLength - readAheadOffset;
                if (readAheadAvailableBytes > 0)
                {
                    var toCopy = Math.Min(readAheadAvailableBytes, count - bytesRead);
                    Array.Copy(readAheadBuffer, readAheadOffset, buffer, offset + bytesRead, toCopy);
                    bytesRead += toCopy;
                    readAheadOffset += toCopy;
                }
                else
                {
                    readAheadOffset = 0;
                    readAheadLength = base.Read(readAheadBuffer, 0, readAheadBuffer.Length);
                    if (readAheadLength == 0)
                    {
                        break;
                    }
                }
            }
            return bytesRead;
        }
    }
}