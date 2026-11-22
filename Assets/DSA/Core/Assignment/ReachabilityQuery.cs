using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace FrontierDispatch.Core
{
    // Assignment Phase 5. Adapt shortest-path reasoning to a travel-cost budget.

    /// <summary>
    /// Finds locations that can be reached from a starting location without
    /// exceeding a maximum total travel cost.
    ///
    /// This is an adaptation of shortest-path reasoning: instead of searching
    /// for one goal, the query determines the cheapest known cost to every
    /// location that fits within the supplied budget.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    ///
    /// What do we mean by "query"? In this context, a query is simply a one-time search for information.
    /// </summary>
    public sealed class ReachabilityQuery
    {
        private readonly WorldData _world;

        /// <summary>
        /// Creates a reachability query operating on the supplied world.
        /// </summary>
        public ReachabilityQuery(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>
        /// Returns reachable location IDs mapped to their cheapest known
        /// travel cost from the starting location.
        ///
        /// Only locations whose cheapest cost is within maxTravelCost should
        /// be included. The starting location has cost 0.
        /// </summary>
        public IReadOnlyDictionary<int, int> FindReachable(int startLocationId, int maxTravelCost)
        {
            var result = new Dictionary<int, int>();

            // negatvice budget cannot cover the start cost zero
            if (maxTravelCost < 0)
            {
                return result;
            }

            // unknown start = nothing to explore
            if (!_world.LocationsById.ContainsKey(startLocationId))
            {
                return result;
            }

            // using same bookkeeping containers as DijkstraRouteFinder
            var dist = new Dictionary<int, int>(_world.LocationsById.Count);
            var settled = new HashSet<int>();
            var queue = new MinPriorityQueue<(int NodeId, int Cost)>();

            dist[startLocationId] = 0;
            queue.Enqueue((startLocationId, 0), 0);

            while (queue.Count > 0)
            {
                (int current, int costAtPush) = queue.Dequeue();

                // superseded entry (a cheaper push arrived first)
                if (costAtPush != dist[current])
                {
                    continue;
                }

                // already finalised
                if (!settled.Add(current))
                {
                    continue;
                }

                result[current] = costAtPush;

                if (!_world.OutGoingConnections.TryGetValue(current, out var edges))
                {
                    continue;
                }

                for (int i = 0; i < edges.Length; ++i)
                {
                    Connection edge = edges[i];
                    int candidate = costAtPush + edge.TravelCost;

                    // budget pruning at relaxation time so out of budget nodes don't occupy heap slots
                    if (candidate > maxTravelCost)
                    {
                        continue;
                    }

                    if (dist.TryGetValue(edge.ToLocationId, out int existing) && candidate >= existing)
                    {
                        continue;
                    }

                    dist[edge.ToLocationId] = candidate;
                    queue.Enqueue((edge.ToLocationId, candidate), candidate);
                }
            }

            return result;
        }
    }
}
