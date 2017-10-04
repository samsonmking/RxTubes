using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RxTubes.MessageTypes;
using System.Reactive.Linq;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using RxTubes.TCP;

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
                .ParseForMessageLength(bytes => BitConverter.ToInt32(bytes, 0))
                .WriteOutputHeader(body =>
                {
                    var header = BitConverter.GetBytes(body.Length + 4);
                    return header.Concat(body).ToArray();
                });

            var localIP = IPAddress.Parse("127.0.0.1");
            var server = new ReactiveListener(localIP, 5000, messageType);

            server.Connections
                .SelectMany(connection =>
                {
                    return connection.WhenMessage
                        .SelectMany(msg =>
                        {
                            var pongBody = Encoding.ASCII.GetBytes("pong");
                            return connection.SendObservableBytes(pongBody);
                        });
                })
                .Subscribe();

            var client = new ReactiveClient(localIP, 5000, messageType);

            var pingBody = Encoding.ASCII.GetBytes("ping");

            var whenClientSends = client.SendObservableBytes(pingBody)
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
        public async Task TestSingleClientServerTerminatorPingPongSendBytes()
        {
            var messageType = new TerminatorMessage()
                .SetMessageTerminator('\r');
            var localIP = IPAddress.Parse("127.0.0.1");
            var server = new ReactiveListener(localIP, 5000, messageType);

            server.Connections
                .SelectMany(connection => connection.WhenMessage.SelectMany(_ => connection.SendObservableBytes(Encoding.ASCII.GetBytes("pong"))))
                .Subscribe();

            var client = new ReactiveClient(localIP, 5000, messageType);
            var whenClientSends = client.SendObservableBytes(Encoding.ASCII.GetBytes("ping"))
                .SelectMany(Observable.Never);

            var reply = await whenClientSends.Merge(client.WhenMessage)
                .Select(bytes => Encoding.ASCII.GetString(bytes).Trim())
                .FirstOrDefaultAsync();

            Assert.AreEqual(reply, "pong");
        }

        [TestMethod]
        public async Task TestSingleClientServerTerminatorPingPongSendString()
        {
            var messageType = new TerminatorMessage()
                .SetMessageTerminator('\r');
            var localIP = IPAddress.Parse("127.0.0.1");
            var server = new ReactiveListener(localIP, 5000, messageType);

            server.Connections
                .SelectMany(connection => connection.WhenMessage.SelectMany(_ => connection.SendObservableString("pong", Encoding.ASCII)))
                .Subscribe();

            var client = new ReactiveClient(localIP, 5000, messageType);
            var whenClientSends = client.SendObservableString("ping", Encoding.ASCII)
                .SelectMany(Observable.Never<byte[]>());

            var reply = await whenClientSends.Merge(client.WhenMessage)
                .Select(bytes => Encoding.ASCII.GetString(bytes).Trim())
                .FirstOrDefaultAsync();

            Assert.AreEqual(reply, "pong");
        }
    }
}
