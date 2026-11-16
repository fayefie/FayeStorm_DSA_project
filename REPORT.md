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

## 3. Algorithms and adaptation

Explain your route finder at a useful high level and how the travel-budget query adapts the same ideas.

## 4. Complexity

| Operation | Your representation/algorithm | Complexity | Short explanation |
| --- | --- | --- | --- |
| Find location by ID | | | |
| Get outgoing connections | | | |
| Priority queue enqueue | | | |
| Priority queue dequeue-min | | | |
| Route finding | | | |

## 5. Correctness and testing

Important tests/edge cases and what they demonstrate.

## 6. Performance investigation

Record reproducible before/after measurements for loading/preparation, repeated runtime work, and relevant allocation behavior. Explain what changed and why.

## 7. Development reflection

Describe one bug, wrong assumption, failed approach, or surprising result and how you investigated it.

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.
