using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    public sealed class WorldData
    {
        private readonly Location[] _locations;
        private readonly Connection[] _connections;
        private readonly Dictionary<int, Location> _locationsById;
        private readonly Dictionary<int, Connection[]> _outGoingByLocation;

        public WorldData(IEnumerable<Location> locations, IEnumerable<Connection> connections)
        {
            if (locations == null) throw new ArgumentNullException(nameof(locations));
            if (connections == null) throw new ArgumentNullException(nameof(connections));
            _locations = new List<Location>(locations).ToArray();
            _connections = new List<Connection>(connections).ToArray();

            _locationsById = new Dictionary<int, Location>(_locations.Length);
            for (int i = 0; i < _locations.Length; ++i)
            {
                _locationsById.TryAdd(_locations[i].Id, _locations[i]);
            }

            var grouped = new Dictionary<int, List<Connection>>();
            for (int i = 0; i < _connections.Length; ++i)
            {
                Connection c = _connections[i];
                if (!grouped.TryGetValue(c.FromLocationId, out List<Connection>? list))
                {
                    grouped[c.FromLocationId] = list = new List<Connection>();
                }
                list.Add(c);
            }
            _outGoingByLocation = new Dictionary<int, Connection[]>(grouped.Count);
            foreach (KeyValuePair<int, List<Connection>> kv in grouped)
            {
                _outGoingByLocation[kv.Key] = kv.Value.ToArray();
            }
        }

        public IReadOnlyList<Location> Locations => _locations;
        public IReadOnlyList<Connection> Connections => _connections;
        public IReadOnlyDictionary<int, Location> LocationsById => _locationsById;
        public IReadOnlyDictionary<int, Connection[]> OutGoingConnections => _outGoingByLocation;
    }
}
