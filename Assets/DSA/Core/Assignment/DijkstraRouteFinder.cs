using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    // Assignment Phase 4.

    /// <summary>
    /// Finds minimum-total-cost routes through a Frontier Dispatch world
    /// using Dijkstra's shortest-path algorithm.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    /// It should eventually use the student's prepared world representation
    /// and MinPriorityQueue implementation.
    /// </summary>
    public sealed class DijkstraRouteFinder
    {
        private readonly WorldData _world;

        /// <summary>
        /// Constructor. Creates a route finder operating on the supplied world.
        /// </summary>
        public DijkstraRouteFinder(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>
        /// Finds a minimum-total-travel-cost route from the start location
        /// to the goal location.
        ///
        /// A successful RouteResult should contain the route as location IDs,
        /// including both the start and goal locations.
        /// </summary>
        public RouteResult FindRoute(int startLocationId, int goalLocationId)
        {
            // both endpoints must be known locations
            if (!_world.LocationsById.ContainsKey(startLocationId) || !_world.LocationsById.ContainsKey(goalLocationId))
            {
                return new RouteResult(false, Array.Empty<int>(), -1, 0);
            }

            // if start == goal. zero cost single node route
            if (startLocationId == goalLocationId)
            {
                return new RouteResult(true, new[] { startLocationId }, 0, 0);
            }

            // dist[v] = best known distance to v
            // prev[v] = predecessor to the best known route to v
            var dist = new Dictionary<int, int>(_world.LocationsById.Count);
            var prev = new Dictionary<int, int>(_world.LocationsById.Count);
            var settled = new HashSet<int>();

            // heap stores nodeid and cost-at-push-time. compare on pop
            var queue = new MinPriorityQueue<(int NodeId, int Cost)>();

            dist[startLocationId] = 0;
            queue.Enqueue((startLocationId, 0), 0);

            int expanded = 0;
            bool reachedGoal = false;

            while (queue.Count > 0)
            {
                (int current, int costAtPush) = queue.Dequeue();

                // skip entries superceded by cheaper push
                if (costAtPush != dist[current])
                {
                    continue;
                }

                // skip nodes that have already been finalised
                if (!settled.Add(current))
                {
                    continue;
                }

                expanded++;

                if (current == goalLocationId)
                {
                    reachedGoal = true;
                    break;
                }

                // prepared outgoing-edge lookup that doesn't rescan the while world
                if (!_world.OutGoingConnections.TryGetValue(current, out var edges))
                {
                    continue;
                }

                for (int i = 0; i < edges.Length; ++i)
                {
                    Connection edge = edges[i];
                    int candidate = costAtPush + edge.TravelCost;

                    if (dist.TryGetValue(edge.ToLocationId, out int existing) && candidate >= existing)
                    {
                        continue;
                    }

                    dist[edge.ToLocationId] = candidate;
                    prev[edge.ToLocationId] = current;
                    queue.Enqueue((edge.ToLocationId, candidate), candidate);
                }
            }

            if (!reachedGoal)
            {
                return new RouteResult(false, Array.Empty<int>(), -1, expanded);
            }

            // reconstruct the route from back to start and then reverse
            var route = new List<int>();
            int node = goalLocationId;
            while (true)
            {
                route.Add(node);
                if (node == startLocationId)
                {
                    break;
                }
                node = prev[node];
            }
            route.Reverse();

            return new RouteResult(true, route, dist[goalLocationId], expanded);
        }
    }
}
