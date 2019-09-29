using System;
using System.IO;
using System.Text;

namespace plr
{
    public class IcyMetaReaderStream : Stream
    {
        protected readonly Stream _sourceStream;
        protected long _pos;

        private int _icyMetaInt;

        public IcyMetaReaderStream(Stream sourceStream, int icyMetaInt)
        {
            _sourceStream = sourceStream;
            _icyMetaInt = icyMetaInt;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override void Flush()
        {
            return;
        }

        public override long Length => _pos;

        public override long Position
        {
            get => _pos;
            set => throw new InvalidOperationException();
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new InvalidOperationException();

        public override void SetLength(long value) =>
            throw new InvalidOperationException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new InvalidOperationException();

        private int receivedBytes;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (receivedBytes == _icyMetaInt && _icyMetaInt > 0)
            {
                var metaLen = _sourceStream.ReadByte();
                if (metaLen > 0)
                {
                    var metaInfo = new byte[metaLen * 16];
                    var len = 0;
                    while ((len += _sourceStream.Read(metaInfo, len, metaInfo.Length - len)) < metaInfo.Length);
                    MetadataHeader = Encoding.UTF8.GetString(metaInfo, 0, metaInfo.Length);
                }

                receivedBytes  = 0;
            }
            var bytesLeft = _icyMetaInt - receivedBytes > count
                ? count
                : _icyMetaInt - receivedBytes;
            var result = _sourceStream.Read(buffer, offset, bytesLeft);
            _pos += result;
            receivedBytes += result;
            return result;
        }

        // parser
        public string MetadataHeader { get; private set; }
    }
}