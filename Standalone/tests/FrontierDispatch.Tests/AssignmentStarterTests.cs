using System;
using NUnit.Framework;
using FrontierDispatch.Core;
using System.Linq;

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

        [Test, Ignore("Phase 4: enable when implementing Dijkstra route finding.")]
        public void Dijkstra_StartEqualsGoal_ReturnsZeroCostRoute()
        {
            var finder = new DijkstraRouteFinder(WorldDatasetFactory.CreateSmall());
            RouteResult result = finder.FindRoute(0, 0);
            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0 }));
        }

        [Test, Ignore("Phase 5: enable when implementing bounded reachability.")]
        public void Reachability_ZeroBudget_IncludesStartAtZeroCost()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, 0);
            Assert.That(result.ContainsKey(0), Is.True);
            Assert.That(result[0], Is.EqualTo(0));
        }
    }
}
