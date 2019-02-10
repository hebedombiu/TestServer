using System;

namespace TestServer {
    internal class Program {
        public static void Main(string[] args) {
            Console.WriteLine("Server started");

            var clientManager = new ClientManager(new PortManager());

            while (true) {
            }
        }
    }
}