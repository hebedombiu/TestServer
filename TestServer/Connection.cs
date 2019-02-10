using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestServer {
    public class Connection {
        private readonly UdpClient _udp;
        
        private readonly Thread _listenThread;
        private readonly Thread _sendThread;
        
        private readonly Action<Connection, Message> _onReceiveAction;
        private readonly Queue<Message> _messages = new Queue<Message>();

        private IPEndPoint _localEp;
        private IPEndPoint _remoteEp;

        private DateTime _lastTime = DateTime.Now;
        
        public IPEndPoint LocalEp => _localEp;
        public IPEndPoint RemoteEp => _remoteEp;
        
        public TimeSpan Delay => DateTime.Now - _lastTime;
        public Client Client { get; }

        public Connection(Action<Connection, Message> action, IPEndPoint localEp, IPEndPoint remoteEp, Client client) {
            Client = client;
            _localEp = localEp;
            _remoteEp = remoteEp;
            _udp = new UdpClient(_localEp);

            _onReceiveAction = action;

            _listenThread = new Thread(ListenThreadStart);
            _listenThread.Start();
            
            _sendThread = new Thread(SendThreadStart);
            _sendThread.Start();
        }

        private void ListenThreadStart() {
            while (true) {
                try {
                    var data = _udp.Receive(ref _remoteEp);
                    
                    var message = new Message(data);
                    _onReceiveAction(this, message);
                    _lastTime = DateTime.Now;
                } catch (SocketException e) {
                    Close();
                }
            }
        }

        private void SendThreadStart() {
            while (true) {
                lock (_messages) {
                    while (_messages.Count > 0) {
                        var message = _messages.Dequeue();
                        try {
                            _udp.Send(message.Bytes, message.Bytes.Length, _remoteEp);
                        } catch (SocketException e) {
                            Close();
                        }
                    }
                }
            }
        }

        public void Send(Message message) {
            lock (_messages) {
                _messages.Enqueue(message);
            }
        }

        public void Close() {
            _listenThread.Abort();
            _sendThread.Abort();
            _udp.Close();
        }
        
        ~Connection() {
            Close();
        }
    }
}