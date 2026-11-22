using System;
using NUnit.Framework;
using FrontierDispatch.Core;
using System.Linq;
using NUnit.Framework.Internal;

namespace FrontierDispatch.Tests
{
    /// <summary>
    /// Small contract examples for later assignment phases.
    ///
    /// These are not an exhaustive assignment test suite.
    /// Students are expected to enable the relevant tests as they reach each
    /// phase and add meaningful tests of their own, including edge cases.
    /// </summary>
    public sealed class AssignmentStarterTests
    {
        // These tests document later assignment contracts without making the
        // untouched baseline red. Remove Ignore when you reach the named phase.

        [Test]
        public void PriorityQueue_EnqueueAndDequeue_ReturnsLowestPriorityFirst()
        {
            var queue = new MinPriorityQueue<string>();
            queue.Enqueue("expensive", 9);
            queue.Enqueue("cheap", 2);
            queue.Enqueue("medium", 5);
            Assert.That(queue.Dequeue(), Is.EqualTo("cheap"));
        }

        [Test]
        public void Stress_EnqueueThenDrain_ReturnsItemsInNonDecreasingPriorityOrder()
        {
            var rng = new Random(20240607);
            const int n = 1000;

            var priorityOf = Enumerable.Range(0, n).ToArray();
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (priorityOf[i], priorityOf[j]) = (priorityOf[j], priorityOf[i]);
            }

            var q = new MinPriorityQueue<int>();
            for (int i = 0; i < n; i++)
            {
                q.Enqueue(i, priorityOf[i]);
            }

            int[] expectedOrder = Enumerable.Range(0, n).OrderBy(i => priorityOf[i]).ToArray();

            for (int k = 0; k < n; k++)
            {
                Assert.That(q.Dequeue(), Is.EqualTo(expectedOrder[k]),
                    $"Dequeue #{k} did not return the next-lowest-priority item.");
            }
            Assert.That(q.Count, Is.EqualTo(0));
        }

        [Test]
        public void SiftDown_AfterDequeue_SwapWithSmallerRightChild_NotJustLeft()
        {
            var q = new MinPriorityQueue<string>();
            q.Enqueue("zero", 0);
            q.Enqueue("five", 5);
            q.Enqueue("one", 1);
            q.Enqueue("ten", 10);

            Assert.That(q.Dequeue(), Is.EqualTo("zero"));
            Assert.That(q.Peek(), Is.EqualTo("one"));
            Assert.That(q.Dequeue(), Is.EqualTo("one"));
            Assert.That(q.Dequeue(), Is.EqualTo("five"));
            Assert.That(q.Dequeue(), Is.EqualTo("ten"));
        }

        [Test]
        public void Dijkstra_StartEqualsGoal_ReturnsZeroCostRoute()
        {
            var finder = new DijkstraRouteFinder(WorldDatasetFactory.CreateSmall());
            RouteResult result = finder.FindRoute(0, 0);
            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void Dijkstra_UnknownStartOrGoal_ReturnsNotFound()
        {
            var finder = new DijkstraRouteFinder(WorldDatasetFactory.CreateSmall());

            const int unknown = int.MaxValue;

            Assert.That(finder.FindRoute(unknown, 0).Found, Is.False, "Unknown start should not produce a route");
            Assert.That(finder.FindRoute(0, unknown).Found, Is.False, "Unknown goal should not produce a route");
            Assert.That(finder.FindRoute(unknown, unknown).Found, Is.False, "Two unknown endpoints should not produce a route");
        }

        [Test]
        public void Reachability_ZeroBudget_IncludesStartAtZeroCost()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, 0);
            Assert.That(result.ContainsKey(0), Is.True);
            Assert.That(result[0], Is.EqualTo(0));
        }

        [Test]
        public void Reachability_NegativeBudget_RetrunsEmpty()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, -1);

            Assert.That(result, Is.Empty, "No location (not even the start location) fits a negative budget");
        }

        [Test]
        public void Reachability_UnknownStart_ReturnsEmpty()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(int.MaxValue, 100);

            Assert.That(result, Is.Empty, "An unknown start must not surface as a reachable node");
        }

        [Test]
        public void Reachability_CostsSatisfyTriangleInequality()
        {
            var world = WorldDatasetFactory.CreateSmall();
            var query = new ReachabilityQuery(world);
            int start = world.LocationsById.Keys.Min();
            int budget = int.MaxValue / 2;

            var reachable = query.FindReachable(start, budget);

            Assert.That(reachable.ContainsKey(start), Is.True);
            Assert.That(reachable[start], Is.EqualTo(0));

            foreach (var kv in reachable)
            {
                if (!world.OutGoingConnections.TryGetValue(kv.Key, out var edges))
                {
                    continue;
                }

                foreach (var e in edges)
                {
                    int viaA = kv.Value + e.TravelCost;
                    if (viaA > budget)
                    {
                        continue;
                    }

                    Assert.That(reachable.ContainsKey(e.ToLocationId), Is.True, $"Edge {kv.Key} -> {e.ToLocationId} fits in the budget but the target wasn't reached");
                    Assert.That(reachable[e.ToLocationId], Is.LessThanOrEqualTo(viaA), $"Non-minimal cost for {e.ToLocationId}: {reachable[e.ToLocationId]} " + $"but route via {kv.Key} gives {viaA}");
                }
            }
        }
    }
}
