using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRChat
{
    public class ConnectionMapping<T>
    {
        private readonly Dictionary<T, HashSet<string>> _connections =
            new Dictionary<T, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(key, connections);
                }

                lock (connections)
                {
                    if (!connections.Contains(connectionId))
                        connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(T key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(T key, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }

        public void RemoveByConnectionId(string connectionId)
        {
            lock (_connections)
            {
                Tuple<T, HashSet<string>> connection = null;
                foreach (var kv in _connections)
                {
                    if (kv.Value.Contains(connectionId))
                        connection = Tuple.Create(kv.Key, kv.Value);
                }
                if (connection == null) return;

                lock (connection.Item2)
                {
                    connection.Item2.Remove(connectionId);

                    if (connection.Item2.Count == 0)
                    {
                        _connections.Remove(connection.Item1);
                    }
                }
            }
        }
    }
}