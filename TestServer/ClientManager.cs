using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;

namespace TestServer {
    public class ClientManager {
        private readonly Dictionary<int, Client> _clients = new Dictionary<int, Client>();
        private readonly UdpClient _udp = new UdpClient(new IPEndPoint(IPAddress.Any, PortManager.MainPort));
        private readonly Thread _registerThread;
        private readonly Thread _positionsThread;
        private readonly Thread _serverInfoThread;
        private readonly Thread _checkConnThread;

        private int _idCounter;

        private List<Vector2> _food = new List<Vector2>();

        private IEnumerable<Vector2> Food {
            get {
                lock (_food) {
                    return new List<Vector2>(_food);
                }
            }
        }
        
        private IEnumerable<Client> Clients {
            get {
                lock (_clients) {
                    return new List<Client>(_clients.Values);
                }
            }
        }

        public ClientManager(PortManager portManager) {
            var random = new Random();
            while (_food.Count < 50) {
                var foodPos = new Vector2(random.Next(-20, 20), random.Next(-20, 20));
                if (_food.Contains(foodPos)) continue;
                _food.Add(foodPos);
            }
            
            _registerThread = new Thread(() => {
                IPEndPoint remoteEp = null;
                
                while (true) {
                    var rMessage = new Message(_udp.Receive(ref remoteEp));
                    if (rMessage.Type == MessageType.Register) {
                        var port = portManager.Get();

                        var client = Create(new IPEndPoint(IPAddress.Any, port), remoteEp);
                        
                        var sMessage = new Message(MessageType.ConnectData, new ConnectData(client.Id, port));
                        
                        _udp.Send(sMessage.Bytes, sMessage.Bytes.Length, remoteEp);
                    }

                    remoteEp = null;
                    
                    Thread.Sleep(200);
                }
            });
            _registerThread.Start();

            _positionsThread = new Thread(() => {
                while (true) {
                    var positions = new FieldPositions();

                    foreach (var client in Clients) {
                        positions.Add(new FieldPosition(FieldPositionType.Player, client.Position, client.Id));
                    }

                    foreach (var position in Food) {
                        positions.Add(new FieldPosition(FieldPositionType.Food, position));
                    }

                    var message = new Message(MessageType.FieldPositions, positions);
                    Broadcast(message);
                    Thread.Sleep(100);
                }
            });
            _positionsThread.Start();
            
            _serverInfoThread = new Thread(() => {
                while (true) {
                    var message = new Message(MessageType.Info, new ServerInfo(_clients.Count, _food.Count));
                    Broadcast(message);
                    Thread.Sleep(200);
                }
            });
            _serverInfoThread.Start();
            
            _checkConnThread = new Thread(() => {
                while (true) {
                    lock (_clients) {
                        foreach (var client in Clients) {
                            if (client.Connection.Delay.Seconds > 10) {
                                _clients.Remove(client.Id);
                                client.Close();
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
            });
            _checkConnThread.Start();
        }

        private void ReceiveMessageAction(Connection connection, Message message) {
            switch (message.Type) {
                case MessageType.Register:
                    break;
                case MessageType.ConnectData:
                    break;
                case MessageType.Text:
                    Broadcast(message);
                    break;
                case MessageType.Vector2:
                    connection.Client.Move(message.Vector2);

                    var pos = connection.Client.Position;
                    var mixX = pos.X - .75f;
                    var maxX = pos.X + .75f;
                    var minY = pos.Y - .75f;
                    var maxY = pos.Y + .75f;

                    foreach (var fPos in _food.ToArray()) {
                        if (fPos.X > mixX && fPos.X < maxX && fPos.Y > minY && fPos.Y < maxY) {
                            _food.Remove(fPos);
                        }
                    }
                    
                    break;
                case MessageType.Int:
                    break;
                case MessageType.Ping:
                    connection.Send(new Message(MessageType.Ping, new byte[] { }));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private Client Create(IPEndPoint localEp, IPEndPoint remoteEp) {
            lock (_clients) {
                var client = new Client(_idCounter++, ReceiveMessageAction, localEp, remoteEp);
                _clients.Add(client.Id, client);
                return client;
            }
        }

        public void Broadcast(Message message) {
            lock (_clients) {
                foreach (var client in _clients.Values) {
                    client.Send(message);
                }
            }
        }

        ~ClientManager() {
            _udp.Close();
            _registerThread.Abort();
            _positionsThread.Abort();
            _serverInfoThread.Abort();
            _checkConnThread.Abort();
        }
    }
}