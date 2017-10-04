using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxTubes.MessageTypes
{
    public interface IMessageType
    {
        Task<byte[]> GetMessageAsync(Stream stream);
        byte[] FormatOutput(byte[] payload);
    }
}
