﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxTubes
{
    public class FixedHeaderMessage : IMessageType
    {
        private int _headerLength;
        private Func<byte[], int> _parser;

        public async Task<byte[]> GetMessageAsync(Stream stream)
        {
            var headerBuffer = new byte[_headerLength];
            await stream.ReadAsync(headerBuffer, 0, _headerLength);
            var messageLenth = _parser(headerBuffer) - _headerLength;
            var messageBuffer = new byte[messageLenth];
            await stream.ReadAsync(messageBuffer, 0, messageLenth);
            return messageBuffer;
        }

        public FixedHeaderMessage SetHeaderLength(int length)
        {
            _headerLength = length;
            return this;
        }

        public FixedHeaderMessage ParseForMessageLength(Func<byte[], int> parser)
        {
            _parser = parser;
            return this;
        }
    }
}