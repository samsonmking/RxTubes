using RxTubes.MessageTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RxTubes.TCP
{
    public class ReactiveClient : ReactiveSocket
    {
        public ReactiveClient(string host, int port, IMessageType messageType) : base(new TcpClient(host, port), messageType)
        {
            
        }
    }
}
