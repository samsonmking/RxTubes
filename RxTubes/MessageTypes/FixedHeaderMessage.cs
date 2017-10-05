using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RxTubes.MessageTypes
{
    public class FixedHeaderMessage : IMessageType
    {
        // Config
        private int _headerLength;
        private Func<byte[], int> _parser;
        private Func<byte[], byte[]> _headerFormatterBytes = bytes => bytes;
        private Func<string, byte[]> _headerFormatterString = msg => Encoding.ASCII.GetBytes(msg);

        public FixedHeaderMessage SetHeaderLength(int length)
        {
            _headerLength = length;
            return this;
        }

        public FixedHeaderMessage SetupOutputBytesWriter(Func<byte[], byte[]> headerFormatter)
        {
            _headerFormatterBytes = headerFormatter;
            return this;
        }

        public FixedHeaderMessage SetupOutputStringWriter(Func<string, byte[]> stringFormatter)
        {
            _headerFormatterString = stringFormatter;
            return this;
        }

        public FixedHeaderMessage SetupMessageLengthParser(Func<byte[], int> parser)
        {
            _parser = parser;
            return this;
        }

        // Interface Members
        public byte[] FormatOutputByte(byte[] payload)
        {
            return _headerFormatterBytes(payload);
        }

        public byte[] FormatOutputString(string payload)
        {
            return _headerFormatterString(payload);
        }

        public async Task<byte[]> GetMessageAsync(Stream stream)
        {
            var headerBuffer = new byte[_headerLength];
            await stream.ReadAsync(headerBuffer, 0, _headerLength);
            var messageLenth = _parser(headerBuffer) - _headerLength;
            var messageBuffer = new byte[messageLenth];
            await stream.ReadAsync(messageBuffer, 0, messageLenth);
            return messageBuffer;
        }
    }
}
