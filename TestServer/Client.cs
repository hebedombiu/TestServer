using System;
using System.Net;
using System.Numerics;

namespace TestServer {
    public class Client {
        private readonly Connection _connection;

        public Connection Connection => _connection;
        public int Id { get; }

        public Vector2 Position { get; private set; }
        
        public Client(int id, Action<Connection, Message> action, IPEndPoint localEp, IPEndPoint remoteEp) {
            Id = id;
            _connection = new Connection(action, localEp, remoteEp, this);
            Position = new Vector2(0,0);
        }

        public void Send(Message message) {
            _connection.Send(message);
        }

        public void Move(Vector2 direction) {
            Position += direction * 0.1f;
        }

        public void Close() {
            _connection.Close();
        }
    }
}