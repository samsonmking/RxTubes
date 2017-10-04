using RxTubes.MessageTypes;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RxTubes.TCP
{
    public class ReactiveListener : IDisposable
    {
        private TcpListener _listener;
        private IMessageType _messageType;
        
        public ReactiveListener(IPAddress localAddress, int port, IMessageType messageType)
        {
            _messageType = messageType;
            _listener = new TcpListener(localAddress, port);
        }

        public IObservable<ReactiveSocket> Connections =>
            Observable.Create<IObservable<ReactiveSocket>>(o =>
            {
                _listener.Start();
                o.OnNext(Observable.FromAsync(async () => await _listener.AcceptTcpClientAsync())
                    .Select(client => new ReactiveSocket(client, _messageType))
                    .Repeat());
                return Disposable.Create(this.Dispose);
            })
            .SelectMany(o => o);

        public void Dispose()
        {
            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }
        }
    }
}
