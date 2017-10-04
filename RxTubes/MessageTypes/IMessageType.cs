using System.IO;
using System.Threading.Tasks;

namespace RxTubes.MessageTypes
{
    public interface IMessageType
    {
        Task<byte[]> GetMessageAsync(Stream stream);
        byte[] FormatOutput(byte[] payload);
    }
}
