using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace RxTubes
{
    public class ReactiveSocket : IDisposable
    {
        private string _host;
        private int _port;
        private TcpClient _client;
        IMessageType _messageType;
        private IConnectableObservable<byte[]> _messages;

        public ReactiveSocket(string host, int port, IMessageType messageType)
        {
            _host = host;
            _port = port;
            _messageType = messageType;
        }

        public IConnectableObservable<byte[]> WhenMessageConnectable
        {
            get
            {
                if (_messages == null)
                {
                    _messages = WhenMessage().Publish();
                }
                return _messages;
            }
        }

        private IObservable<byte[]> WhenMessage()
        {
            return Observable.Create<IObservable<byte[]>>(async o =>
            {
                if (_client != null) o.OnError(new Exception("Connection already in use"));
                _client = new TcpClient();
                await _client.ConnectAsync(_host, _port);
                if (!_client.Connected) o.OnError(new Exception("Failed to connect to host"));
                var stream = GetStream();
                o.OnNext(Observable.FromAsync(async () => await _messageType.GetMessageAsync(stream)).Repeat());
                return Disposable.Create(this.Dispose);
            })
            .SelectMany(o => o);
        }

        public IObservable<byte[]> SendObservableBytes(byte[] payload)
        {
            var stream = GetStream();
            return Observable.FromAsync(async () =>
            {
                await stream.WriteAsync(payload, 0, payload.Length);
                return payload;
            });
        }

        public IObservable<string> SendObservableString(string msg, Encoding encoding)
        {
            var stream = GetStream();
            var msgAsBytes = encoding.GetBytes(msg);
            return Observable.FromAsync(async () =>
            {
                await stream.WriteAsync(msgAsBytes, 0, msgAsBytes.Length);
                return msg;
            });
        }

        public async Task SendBytesAsync(byte[] payload)
        {
            var stream = GetStream();
            await stream.WriteAsync(payload, 0, payload.Length);
        }

        public async Task SendStringAsync(string msg, Encoding encoding)
        {
            var stream = GetStream();
            var msgAsBytes = encoding.GetBytes(msg);
            await stream.WriteAsync(msgAsBytes, 0, msgAsBytes.Length);
        }

        protected virtual Stream GetStream()
        {
            return _client.GetStream();
        }

        public void Dispose()
        {
            if (_client.Connected)
            {
                _client.Close();
            }
        }
    }
}
