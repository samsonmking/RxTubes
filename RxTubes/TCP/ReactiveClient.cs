using RxTubes.MessageTypes;
using System.Net.Sockets;

namespace RxTubes.TCP
{
    public class ReactiveClient : ReactiveSocket
    {
        public ReactiveClient(string host, int port, IMessageType messageType) : base(new TcpClient(host, port), messageType)
        {
            
        }
    }
}
