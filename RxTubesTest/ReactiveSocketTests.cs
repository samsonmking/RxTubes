using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RxTubes;
using RxTubes.MessageTypes;
using System.Reactive.Linq;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace RxTubesTest
{
    [TestClass]
    public class ReactiveSocketTests
    {
        [TestMethod]
        public async Task TestSingleClientServerFixedHeaderPingPong()
        {
            var messageType = new FixedHeaderMessage()
                .SetHeaderLength(4)
                .ParseForMessageLength(bytes => BitConverter.ToInt32(bytes, 0));
            var localIP = IPAddress.Parse("127.0.0.1");
            var server = new ReactiveListener(localIP, 5000, messageType);

            server.Connections
                .SelectMany(connection =>
                {
                    return connection.WhenMessage
                        .SelectMany(msg =>
                        {
                            var pongBody = Encoding.ASCII.GetBytes("pong");
                            var pongHeader = BitConverter.GetBytes(pongBody.Length + 4);
                            var pongPayload = pongHeader.Concat(pongBody).ToArray();
                            return connection.SendObservableBytes(pongPayload);
                        });
                })
                .Subscribe();

            var client = new ReactiveClient("127.0.0.1", 5000, messageType);

            var body = Encoding.ASCII.GetBytes("ping");
            var header = BitConverter.GetBytes(body.Length);
            var payload = header.Concat(body).ToArray();

            var whenClientSends = client.SendObservableBytes(payload)
                .SelectMany(Observable.Never);

            var reply = await whenClientSends.Merge(client.WhenMessage)
                .Select(bytes =>
                {
                    return Encoding.ASCII.GetString(bytes);
                })
                .FirstOrDefaultAsync();
            Assert.AreEqual(reply, "pong");
        }

        [TestMethod]
        public async Task TestSingleClientServerTerminatorPingPong()
        {
            var messageType = new TerminatorMessage()
                .SetMessageTerminator('\r');
            var localIP = IPAddress.Parse("127.0.0.1");
            var server = new ReactiveListener(localIP, 5000, messageType);

            server.Connections
                .SelectMany(connection => connection.WhenMessage.SelectMany(_ => connection.SendObservableBytes(Encoding.ASCII.GetBytes("pong\r"))))
                .Subscribe();

            var client = new ReactiveClient("127.0.0.1", 5000, messageType);
            var whenClientSends = client.SendObservableBytes(Encoding.ASCII.GetBytes("ping\r"))
                .SelectMany(Observable.Never);

            var reply = await whenClientSends.Merge(client.WhenMessage)
                .Select(bytes => Encoding.ASCII.GetString(bytes))
                .FirstOrDefaultAsync();

            Assert.AreEqual(reply, "pong\r");
        }
    }
}
