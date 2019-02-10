using System.Collections.Generic;
using System.Linq;

namespace TestServer {
    public class PortManager {
        public const int MainPort = 57099;

        private readonly HashSet<int> _ports = new HashSet<int> {
            57101,
            57102,
            57103,
            57104,
            57105,
            57106,
            57107,
            57108,
            57109,
            57110
        };

        public int Get() {
            lock (_ports) {
                if (_ports.Count == 0) return 0;

                var port = _ports.ToArray()[0];
                _ports.Remove(port);
                return port;
            }
        }

        public void Release(int port) {
            lock (_ports) {
                _ports.Add(port);
            }
        }
    }
}