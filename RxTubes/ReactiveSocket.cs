using RxTubes.MessageTypes;
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
        private TcpClient _client;
        IMessageType _messageType;
        private IObservable<byte[]> _messages;
        private IConnectableObservable<byte[]> _messagesConnectable;

        public ReactiveSocket(TcpClient client, IMessageType messageType)
        {
            _client = client;
            _messageType = messageType;
        }

        public IConnectableObservable<byte[]> WhenMessageConnectable
        {
            get
            {
                if (_messages == null)
                {
                    _messages = GetWhenMessage();
                    _messagesConnectable = _messages.Publish();
                }
                return _messagesConnectable;
            }
        }

        public IObservable<byte[]> WhenMessage
        {
            get
            {
                if (_messages == null)
                {
                    _messages = GetWhenMessage();
                }
                return _messages;
            }
        }

        private IObservable<byte[]> GetWhenMessage()
        {
            return Observable.Create<IObservable<byte[]>>(o =>
            {
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
                var formattedPayload = _messageType.FormatOutput(payload);
                await stream.WriteAsync(formattedPayload, 0, formattedPayload.Length);
                return formattedPayload;
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
