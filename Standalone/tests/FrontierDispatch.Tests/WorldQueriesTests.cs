using System;
using NUnit.Framework;
using FrontierDispatch.Core;

namespace FrontierDispatch.Tests
{
    public sealed class WorldQueriesTests
    {
        [Test]
        public void FindLocationById_ReturnsMatchingLocation()
        {
            var queries = new WorldQueries(SampleWorldFactory.CreateSmall());
            Location? location = queries.FindLocationById(734);
            Assert.That(location, Is.Not.Null);
            Assert.That(location!.Name, Is.EqualTo("North Gate"));
        }

        [Test]
        public void FindLocationById_ReturnsNullWhenMissing()
        {
            var queries = new WorldQueries(SampleWorldFactory.CreateSmall());
            Assert.That(queries.FindLocationById(9999), Is.Null);
        }

        [Test]
        public void FindOutgoingConnections_ReturnsOnlyMatchingOrigin()
        {
            var queries = new WorldQueries(SampleWorldFactory.CreateSmall());
            var outgoing = queries.FindOutgoingConnections(734);
            Assert.That(outgoing.Count, Is.EqualTo(3));
            for (int i = 0; i < outgoing.Count; ++i)
            {
                Assert.That(outgoing[i].FromLocationId, Is.EqualTo(734));
            }
        }

        [Test]
        public void LocationsById_ContainsEverySampleLocation()
        {
            var world = SampleWorldFactory.CreateSmall();

            Assert.That(world.LocationsById.Count, Is.EqualTo(world.Locations.Count));
            foreach (Location location in world.Locations)
            {
                Assert.That(world.LocationsById.TryGetValue(location.Id, out Location? indexed), Is.True);
                Assert.That(indexed, Is.SameAs(location));
            }
        }

        [Test]
        public void LocationsById_ReturnsNullWhenWorldIsEmpty()
        {
            var world = new WorldData(Array.Empty<Location>(), Array.Empty<Connection>());

            var queries = new WorldQueries(world);

            Assert.That(queries.FindLocationById(1), Is.Null);
        }

        [Test]
        public void LocationsById_HandlesDuplicateLocationEntries()
        {
            var originWorld = SampleWorldFactory.CreateSmall();
            Location location = originWorld.Locations[0];

            var world = new WorldData(new[] { location, location }, Array.Empty<Connection>());

            var queries = new WorldQueries(world);

            Assert.That(queries.FindLocationById(location.Id), Is.SameAs(location));
            Assert.That(world.LocationsById.Count, Is.EqualTo(1));
        }
    }
}
