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
        public int BufferSize { get; set; } = 8192;

        public char MessageTerminator { get; set; }

        public byte[] FormatOutput(byte[] payload)
        {
            return payload.Concat(BitConverter.GetBytes(MessageTerminator)).ToArray();
        }

        public async Task<byte[]> GetMessageAsync(Stream stream)
        {
            var buffer = new byte[BufferSize];
            var i = 0;
            byte lastRead = 0;
            while(lastRead != MessageTerminator)
            {
                await stream.ReadAsync(buffer, i, 1);
                lastRead = buffer[i];
                i++;
            }
            var result = new byte[i];
            Array.Copy(buffer, result, i);
            return result;
        }

        public TerminatorMessage SetMessageTerminator(char terminator)
        {
            MessageTerminator = terminator;
            return this;
        }

        public TerminatorMessage SetBufferSize(int bufferSize)
        {
            BufferSize = bufferSize;
            return this;
        }
    }
}
