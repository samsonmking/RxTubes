﻿using RxTubes.MessageTypes;
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

namespace RxTubes.TCP
{
    public class ReactiveSocket : ReactiveSocketBase
    {
        private TcpClient _client;

        public ReactiveSocket(TcpClient client, IMessageType messageType) : base (messageType)
        {
            _client = client;
        }

        protected override void Close()
        {
            _client.Close();
        }

        protected override Stream GetStream()
        {
            return _client.GetStream();
        }

        protected override bool IsConnected()
        {
            return _client.Connected;
        }
    }
}
