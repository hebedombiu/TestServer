namespace TestServer {
    public struct ConnectData {
        public int ServerPort { get; }
        public int Id { get; }
        
        public ConnectData(int id, int serverPort) {
            Id = id;
            ServerPort = serverPort;
        }
    }
}