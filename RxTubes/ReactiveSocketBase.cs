using RxTubes.MessageTypes;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace RxTubes
{
    public abstract class ReactiveSocketBase : IDisposable
    {
        IMessageType _messageType;
        private IObservable<byte[]> _messages;
        private IConnectableObservable<byte[]> _messagesConnectable;

        public ReactiveSocketBase(IMessageType messageType)
        {
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
                if (!IsConnected()) o.OnError(new Exception("Failed to connect to host"));
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
                var formattedPayload = _messageType.FormatOutputByte(payload);
                await stream.WriteAsync(formattedPayload, 0, formattedPayload.Length);
                return formattedPayload;
            });
        }

        public IObservable<string> SendObservableString(string msg)
        {
            var stream = GetStream();
            var msgAsBytes = _messageType.FormatOutputString(msg);
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

        public async Task SendStringAsync(string msg)
        {
            var stream = GetStream();
            var msgAsBytes = _messageType.FormatOutputString(msg);
            await stream.WriteAsync(msgAsBytes, 0, msgAsBytes.Length);
        }

        protected abstract bool IsConnected();

        protected abstract Stream GetStream();

        protected abstract void Close();

        public virtual void Dispose()
        {
            if (IsConnected())
            {
                Close();
            }
        }
    }
}
