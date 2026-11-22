# REPORT.md - Individual Assignment

Keep this concise and specific to your implementation.

## 1. Initial analysis

Baseline results on home PC:  
  Locations: 2500  
  Connections: 9800  
  Iterations: 5000  
  Elapsed: 530 ms  
  Checksum: 6267100  

- How are locations and connections represented in the baseline?  
  - Locations and connections are two flat arrays: `Location[] _locations` and `Connection[] _connection`.  
  - Location holds: `Id`, `Name`, `Position`, and `Type`.
  - Connection is a directed edge with three ints: `FromLocationId`, `ToLocationId`, and `TravelCost`
  - These are used by WorldQueries, where `FindLocationById` does a linear scan of all locations, and `FindOutgoingConnections` does a linear scan of all connections.
    
- Which operations appear frequent?  
  - `FindLocationById(int id)` will be used quite often, as it is scanning through all locations and comparing them with the requested location ID to return the matching locaiton
  - `FindOutgoingConnections(int fromLocationId)` also seems like it will be frequent since it's iterating through all the connections, checking which connections start at the requested location, and then adds the matching connections to a result list.
 
- Which operation(s) do you predict may scale poorly?
  - Both connections seem like they might scale poorly, I think:
  - `FindLocationById(int id)` will scale poorly because it is using a linear search. So everytime the method is called, it will start at the beginning of the collection and compare the requested ID to the ID of each location until it finds the correct one.
    - This might be fine with only a few locations, but when we scale up the locations, the time complexity scales with it:
      - If we have a world with 5000 locations and the requested ID happens to be towards the end, then we're doing thousands of comparisons.
      - Otherwise if the requested ID doesn't exist, then we're going to run through the entire list and examining each ID before we return an error.
    - And also, if we need to retrieve locations using this method frequently, then the collection will be searched repeatedly each time, even if the collection remains unchanged between method calls, which is obviously time costly and inoptimal. 
  - `FindOutgoingConnections` I think the issue with this one is that it also examines every connection in the world, even though it only needs connections from one origin location
    - Scaled up, and with a world of say 2000 connections, and where our particular location only has five outgoing connections, we will still need to examine all 2000 to find the relevant five.
    - Like the previous operation, we are also repeating the scan of the while collection each time, which is again also expensive.
    - It's also using up a lot of memory since the method creates a new `List<Connections>` on each call and adds the matching connections to that list.
- What will you investigate first?  
  - I will start with looking at `FindLocationById`, a dictionary would be a better way to map location IDs and eliminate the repeated searching of locations that's happening each time we call the method.
  - Afterwards `FindOutgoingConnections` might be a good target to follow up with. If my understanding is correct then a dictionary might also be very useful here.

## 2. Data representation

Describe important changes/retained choices, operations affected, and time/memory trade-offs.

- WorldData now builds a dictionary once in its constructor, keyed by Location.Id. the _locations array and Location list are unchanged. The dictionary is exposed as IReadOnlyDictionary<int, Location>, WorldQueries.FindLocationById reads the index with TryGetValue. Instead of scanning the list, so lookups no longer re-scan locations
  - FindLocationById drops from O(L) per call to O(1) average, and the index is build once per world.

- Outgoing connections are now also indexed once in WorldData as Dictionary<int, Connection[]>.
    - FindOutGoingConnections is now an O(1) hash lookup that returns a shared immutable list.

## 3. Algorithms and adaptation

Explain your route finder at a useful high level and how the travel-budget query adapts the same ideas.

`DijkstraRouteFinder.FindRoute(start, goal)` that runs over the outgoing connections index from phase 2 and finds a lowest cost travel route between a start and goal point.

### Bookkeeping
- `dist` - Dictionary<int, int> mapping node id. Current best known distance
- `prev` - Dictionary<int, int> mapping node id. Predecessor to the best known route
- `settled` - HashSet<int> tracking nodes whose distance is already final
- Frontier - `MinPriorityQueuez(int NodeId, int Cost)>, priority = cost

### loop invariants and shortcuts
- Each heap entry stores the node id and cost at the time of push. On dequeue, if the `costAtPush != dist[cureent]` then the entry has been superseded by a cheaper push and is discared (aka laze deletion)
- That, combined with `settled`, each vertex is expanded at moost once
- As soon as the goal is dequeued it is final (Dijkstra's invarint), so the rest of the reachable set is left unexpanded
- Unknown endpoints (those not in `WorldData.LocationsById`) "short circuit" to a not-found result wotj amd empty route and `-1` cost before any heap work
- `start == goal` returns `{start}` with cost 0 without a heap cycle

Route reconstruction then walks `prev` from the goal back to the start, and reversed the list. The reported `TotalCost` is `Dist[goal]`. The same value the seach proved minimal

## 4. Complexity

| Operation | Your representation/algorithm | Complexity | Short explanation |
| --- | --- | --- | --- |
| Find location by ID | Dictionary<int, Location> built once in WorldData | O(1) average per lookup, O(L) build, and O(L) space | One hash lookup on the ID. The linear scan is paid once per world instead of once per query |
| Get outgoing connections | Dictionary<int, Connection[]> | O(1) average per call, O(V+E) build/space | One lookup returns the stored array, and the baseline scans all E edges and allocates a new list each call |
| Priority queue enqueue | Binary min-heap in `List<Entry>` (append + SiftUp) | O(log n) | New entry is appended to the end of the array, then bubbled up until its parent has a lower-or-equal priority |
| Priority queue dequeue-min | Binary min-heap (swap root with last, remove last slow, SiftDown) | O(log n) | Root (minimum) is read out, the last leaf is moved to the root slot, the array shrinks by one, and then the new root is sifted down towards the samller child |
| Route finding | Dijkstra: `Dictionary<int, int>` for `dist/prev`, `MinPriorityQueue<(NodeId, Cost)>` frontier, `HashSet<int>` settled set | O((V + E) log V) time. O(V) space | Each vertex dequeues at most once (superceded entry check + settled set). Each outgoing edge is relaxed at most once. Each heap push/pop is O(log V). Early exit at the goal typically shrinks the expanded set well below V but does not change the worst case |

## 5. Correctness and testing

Important tests/edge cases and what they demonstrate.

1. LocationsById_ContainsEverySampleLocation()
   - Tests the WorldData index that our lookups read from. If there's a broken index (count, dropped ID, etc), it'll fail here first
2. FindLocationById_ReturnsNullWhenWorldIsEmpty()
   - Makes sure that the lookup doesn't throw/return and invalid value if the dictionary has no locations
3. FindLocationById_HandlesDuplicateLocationEntries()
   - Handles having locations ID locations. Since we're using TryAdd then the first location with the sought ID wins
4. FindOutgoingConnections_ReturnsEmptyWhenWorldHasNoConnections()
   - Returns and empty collection when there are no connections in the world
5. Stress_EnqueueThenDrain_ReturnsItemsInNonDecreasingPriorityOrder()
   - Test checks wheather MinPriorityQueue<int> can correctly maintain min-priority order when it's handling a large number of items
6. SiftDown_AfterDequeue_SwapWithSmallerRightChild_NotJustLeft()
   - Checking if `SiftDown()` correctly chooses the smaller of the left and right children when restoring the min-heap after a dequeue.
   - This is to prevent a bug where the implementation always swaps with the left child, even if the right child has a smaller priority
7. Dijkstra_UnknownStartOrGoal_ReturnsNotFound()
   - Test does a missing bounds check, or `ContainsKey` returning wrong value


## 6. Performance investigation

Record reproducible before/after measurements for loading/preparation, repeated runtime work, and relevant allocation behavior. Explain what changed and why.

### Before/After each iteration

| Version | Elapsed (ms)| Checksum |
| --- | --- | --- |
| Before (Baseline) | 530 | 6267100 |
| After (FindLocationById dictionary) | 463 | 6267100 |
| After (GetOutGoingConnections dictionary) | 0 | 6267100 |

- Running the baseline before/after the dictionary implementation for FindLocationById produced a ~13.4% improvement in run speed, showing that the implementation is a new improvement
- After implementing the dictionary for FindOutGoingConnections, the elapsed time when running --baseline is now down to 0ms. Since the workload is mostly dominated by FindOutgoingConnections, and we have 9800-scans per call that have now become hash hits, it makes sense that we've had such a huge improvement now.

## 7. Development reflection

Describe one bug, wrong assumption, failed approach, or surprising result and how you investigated it.

There was an incorrect implementation in my MinPriorityQueue, specifically in the SiftDown(int index) method. The method had the line `int right = 1 * index + 2;` which was calculating the wrong right child element for pretty much everry node.
The bug went unnoticed when I initially wrote the code, since the supplied starter test passed without issue, but it was caught only after a second test was added to test if the prio queue can correctly maintain min-priority order when it's handling a large number of items´.
The error message said "Dequeue #3 did not return the next-lowest-priority item." and where it expected 453 it returned 819, which showed a sifting error. Checking the lab 04b, I found that the correct calculation should actually be `int right = 2 * index + 2;` to calculate the right child correctly.
After changing that line of code, re-running the test again showed all greens.

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.
