#nullable enable

using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    /// <summary>
    /// Straightforward queries over the supplied WorldData.
    ///
    /// The starter project intentionally performs these queries directly over the stored
    /// collections. The implementation is correct, but students should analyze
    /// how its work scales as the world becomes larger and queries are repeated.
    ///
    /// Do not assume that every method must be replaced. Identify the operations
    /// that matter first, then justify any representation changes.
    /// </summary>
    public sealed class WorldQueries
    {
        private readonly WorldData _world;
        private static readonly Connection[] NoConnections = Array.Empty<Connection>();

        public WorldQueries(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public Location? FindLocationById(int id) => _world.LocationsById.TryGetValue(id, out Location? location) ? location : null;

        public IReadOnlyList<Connection> FindOutgoingConnections(int fromLocationId) => _world.OutGoingConnections.TryGetValue(fromLocationId, out Connection[]? edges) ? edges : NoConnections;
    }
}
