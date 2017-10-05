using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxTubes.MessageTypes
{
    public class TerminatorMessage : IMessageType
    {

        // Config
        public int BufferSize { get; set; } = 8192;

        public string MessageTerminator { get; set; } = Environment.NewLine;

        public Encoding Encoding { get; set; } = Encoding.ASCII;

        public TerminatorMessage SetMessageTerminator(string terminator)
        {
            MessageTerminator = terminator;
            return this;
        }

        public TerminatorMessage SetEncoding(Encoding encoding)
        {
            Encoding = encoding;
            return this;
        }

        public TerminatorMessage SetBufferSize(int bufferSize)
        {
            BufferSize = bufferSize;
            return this;
        }

        // Interface Members
        public byte[] FormatOutputByte(byte[] payload)
        {
            return payload.Concat(Encoding.GetBytes(MessageTerminator)).ToArray();
        }

        public byte[] FormatOutputString(string payload)
        {
            return Encoding.ASCII.GetBytes(payload + MessageTerminator);
        }

        public async Task<byte[]> GetMessageAsync(Stream stream)
        {
            var buffer = new byte[BufferSize];
            var i = 0;
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer, i, 1);
                if (bytesRead == 0) break;
                i++;
                var readBytes = new byte[i];
                Array.Copy(buffer, readBytes, i);
                if (Encoding.GetString(readBytes).EndsWith(MessageTerminator)) break;
            }
            var result = new byte[i];
            Array.Copy(buffer, result, i);
            return result;
        }
    }
}
