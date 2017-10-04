using System;
using System.IO;
using System.Threading.Tasks;

namespace RxTubes.MessageTypes
{
    public class FixedHeaderMessage : IMessageType
    {
        private int _headerLength;
        private Func<byte[], int> _parser;
        private Func<byte[], byte[]> _headerFormatter;

        public async Task<byte[]> GetMessageAsync(Stream stream)
        {
            var headerBuffer = new byte[_headerLength];
            await stream.ReadAsync(headerBuffer, 0, _headerLength);
            var messageLenth = _parser(headerBuffer) - _headerLength;
            var messageBuffer = new byte[messageLenth];
            await stream.ReadAsync(messageBuffer, 0, messageLenth);
            return messageBuffer;
        }

        public FixedHeaderMessage SetHeaderLength(int length)
        {
            _headerLength = length;
            return this;
        }

        public FixedHeaderMessage WriteOutputHeader(Func<byte[], byte[]> headerFormatter)
        {
            _headerFormatter = headerFormatter;
            return this;
        }

        public FixedHeaderMessage ParseForMessageLength(Func<byte[], int> parser)
        {
            _parser = parser;
            return this;
        }

        public byte[] FormatOutput(byte[] payload)
        {
            return _headerFormatter(payload);
        }
    }
}
