using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TestServer {
    public class Message {
        public MessageType Type { get; }
        public byte[] Bytes { get; }
        
        public Vector2 Vector2 {
            get {
                var bytesArray = new List<byte>(Bytes);
                bytesArray.RemoveRange(0, 1);

                var x = BitConverter.ToSingle(bytesArray.GetRange(0, 4).ToArray(), 0);
                bytesArray.RemoveRange(0, 4);
                
                var y = BitConverter.ToSingle(bytesArray.GetRange(0, 4).ToArray(), 0);
                bytesArray.RemoveRange(0, 4);

                return new Vector2(x, y);
            }
        }

        private byte[] GetBytes(int data) {
            return BitConverter.GetBytes(data);
        }

        private byte[] GetBytes(float data) {
            return BitConverter.GetBytes(data);
        }

        private byte[] GetBytes(string data) {
            return Encoding.Unicode.GetBytes(data);
        }

        private byte[] GetBytes(Vector2 data) {
            var bytes = new List<byte>();
            
            bytes.AddRange(GetBytes((float) data.X));
            bytes.AddRange(GetBytes((float) data.Y));

            return bytes.ToArray();
        }
        
        private byte[] GetBytes(ConnectData data) {
            var bytes = new List<byte>();
            
            bytes.AddRange(GetBytes(data.Id));
            bytes.AddRange(GetBytes(data.ServerPort));

            return bytes.ToArray();
        }
        
        private byte[] GetBytes(FieldPosition fieldPosition) {
            var bytes = new List<byte>();
            
            bytes.Add(fieldPosition.Type);
            bytes.AddRange(GetBytes(fieldPosition.Position));
            bytes.AddRange(GetBytes(fieldPosition.Id));

            return bytes.ToArray();
        }

        private byte[] GetBytes(FieldPositions fieldPositions) {
            var bytes = new List<byte>();

            foreach (var fieldPosition in fieldPositions) {
                bytes.AddRange(GetBytes(fieldPosition));
            }

            return bytes.ToArray();
        }

        private byte[] GetBytes(ServerInfo serverInfo) {
            var bytes = new List<byte>();
            
            bytes.AddRange(GetBytes(serverInfo.PlayerCount));
            bytes.AddRange(GetBytes(serverInfo.FoodCount));

            return bytes.ToArray();
        }

        public Message(MessageType type, object data) {
            Type = type;
            
            var msg = new List<byte> {(byte) type};

            switch (type) {
                case MessageType.Register:
                    break;
                case MessageType.ConnectData:
                    msg.AddRange(GetBytes((ConnectData) data));
                    break;
                case MessageType.Text:
                    msg.AddRange(GetBytes((string) data));
                    break;
                case MessageType.Vector2:
                    msg.AddRange(GetBytes((Vector2) data));
                    break;
                case MessageType.Int:
                    msg.AddRange(GetBytes((int) data));
                    break;
                case MessageType.FieldPositions:
                    msg.AddRange(GetBytes((FieldPositions) data));
                    foreach (var fieldPosition in (FieldPositions) data) {
                        msg.AddRange(GetBytes(fieldPosition));
                    }
                    break;
                case MessageType.Ping:
                    break;
                case MessageType.Info:
                    msg.AddRange(GetBytes((ServerInfo) data));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            Bytes = msg.ToArray();
        }

        public Message(byte[] data) {
            Type = (MessageType) data[0];
            Bytes = data;
        }

        public override string ToString() {
            return $"Message: {{type: {Type}}}";
        }
    }
}