using System.Threading;
using SmartX.Api.Models;
using SmartX.Api.Repositories;

namespace SmartX.Api.Services
{
    public class MeshGraphService
    {
        private readonly ISensorRepository _sensorRepository;

        // Sensor ID -> mesh node
        private readonly Dictionary<int, MeshNode> _nodes = new();

        // Sensor ID -> list of neighbouring connections
        private readonly Dictionary<int, List<MeshEdge>>
            _adjacencyList = new();

        private readonly List<MeshEdge> _edges = new();

        private bool _graphInitialized = false;

        private readonly SemaphoreSlim _graphLock = new(1, 1);

        private async Task<T> ExecuteWithGraphLockAsync<T>(
            Func<T> operation)
        {
            await _graphLock.WaitAsync();

            try
            {
                return operation();
            }
            finally
            {
                _graphLock.Release();
            }
        }

        private async Task ExecuteWithGraphLockAsync(
            Action operation)
        {
            await _graphLock.WaitAsync();

            try
            {
                operation();
            }
            finally
            {
                _graphLock.Release();
            }
        }


        public MeshGraphService(
            ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        // Build a simulated mesh from registered sensors.


        public async Task BuildGraphAsync()
        {
            await _graphLock.WaitAsync();

            try
            {
                await SynchronizeGraphAsync();
            }
            finally
            {
                _graphLock.Release();
            }
        }

        private async Task SynchronizeGraphAsync()
        {

            var sensors = (await _sensorRepository.GetAllAsync())
                .OrderBy(s => s.Id)
                .ToList();

            // If the graph has never been created,
            // initialise it using the existing logic.
            if (!_graphInitialized)
            {
                _nodes.Clear();
                _adjacencyList.Clear();
                _edges.Clear();

                foreach (var sensor in sensors)
                {
                    _nodes[sensor.Id] = new MeshNode
                    {
                        SensorId = sensor.Id,
                        MacAddress = sensor.MacAddress,
                        DeviceName = sensor.DeviceName,
                        Zone = sensor.Zone,
                        Status = sensor.CalculateStatus(),
                        IsGateway = false
                    };

                    _adjacencyList[sensor.Id] =
                        new List<MeshEdge>();
                }

                if (sensors.Count > 0)
                {
                    _nodes[sensors[0].Id].IsGateway = true;
                }

                for (int i = 0; i < sensors.Count; i++)
                {
                    if (i + 1 < sensors.Count)
                    {
                        AddConnection(
                            sensors[i].Id,
                            sensors[i + 1].Id,
                            2 + (i % 4));
                    }

                    if (i + 2 < sensors.Count)
                    {
                        AddConnection(
                            sensors[i].Id,
                            sensors[i + 2].Id,
                            5 + (i % 3));
                    }
                }

                _graphInitialized = true;
                return;
            }

            // Synchronise existing sensor information.
            foreach (var sensor in sensors)
            {
                if (_nodes.TryGetValue(
                    sensor.Id, out var existingNode))
                {
                    existingNode.MacAddress =
                        sensor.MacAddress;

                    existingNode.DeviceName =
                        sensor.DeviceName;

                    existingNode.Zone =
                        sensor.Zone;

                    existingNode.Status =
                        sensor.CalculateStatus();
                }
                else
                {
                    // Add newly registered sensors.
                    _nodes[sensor.Id] = new MeshNode
                    {
                        SensorId = sensor.Id,
                        MacAddress = sensor.MacAddress,
                        DeviceName = sensor.DeviceName,
                        Zone = sensor.Zone,
                        Status = sensor.CalculateStatus()
                    };

                    _adjacencyList[sensor.Id] =
                        new List<MeshEdge>();

                    // Connect the new sensor to existing nodes.
                    var existingIds = _nodes.Keys
                        .Where(id => id != sensor.Id)
                        .OrderByDescending(id => id)
                        .Take(2)
                        .ToList();

                    foreach (var existingId in existingIds)
                    {
                        AddConnection(
                            sensor.Id,
                            existingId,
                            3);
                    }
                }
            }

            // Remove sensors that no longer exist.
            var currentIds = sensors
                .Select(s => s.Id)
                .ToHashSet();

            var removedIds = _nodes.Keys
                .Where(id => !currentIds.Contains(id))
                .ToList();

            foreach (var removedId in removedIds)
            {
                _nodes.Remove(removedId);
                _adjacencyList.Remove(removedId);

                _edges.RemoveAll(e =>
                    e.SourceSensorId == removedId ||
                    e.DestinationSensorId == removedId);
            }

            // Remove deleted connections from adjacency lists.
            foreach (var pair in _adjacencyList)
            {
                pair.Value.RemoveAll(e =>
                    !_edges.Contains(e));
            }

            // Ensure the lowest-ID sensor is the gateway.
            foreach (var node in _nodes.Values)
            {
                node.IsGateway = false;
            }

            if (_nodes.Count > 0)
            {
                int gatewayId = _nodes.Keys.Min();
                _nodes[gatewayId].IsGateway = true;
            }
        }


        // Add an undirected weighted connection.
        public void AddConnection(
            int sourceSensorId,
            int destinationSensorId,
            double weight)
        {
            if (!_nodes.ContainsKey(sourceSensorId) ||
                !_nodes.ContainsKey(destinationSensorId))
            {
                throw new ArgumentException(
                    "Both sensors must exist in the graph.");
            }

            if (sourceSensorId == destinationSensorId)
            {
                throw new ArgumentException(
                    "A sensor cannot connect to itself.");
            }

            if (weight <= 0 ||
                double.IsNaN(weight) ||
                double.IsInfinity(weight))
            {
                throw new ArgumentException(
                    "Connection weight must be positive and finite.");
            }

            bool alreadyExists = _edges.Any(e =>
                (e.SourceSensorId == sourceSensorId &&
                 e.DestinationSensorId == destinationSensorId) ||
                (e.SourceSensorId == destinationSensorId &&
                 e.DestinationSensorId == sourceSensorId));

            if (alreadyExists)
            {
                return;
            }

            var edge = new MeshEdge
            {
                SourceSensorId = sourceSensorId,
                DestinationSensorId = destinationSensorId,
                Weight = weight,
                IsActive = true
            };

            _edges.Add(edge);

            _adjacencyList[sourceSensorId].Add(edge);
            _adjacencyList[destinationSensorId].Add(edge);
        }

        public List<MeshNode> GetNodes()
        {
            return _nodes.Values
                .OrderBy(n => n.SensorId)
                .ToList();
        }

        public List<MeshEdge> GetEdges()
        {
            return _edges.ToList();
        }

        public MeshNode? GetNode(int sensorId)
        {
            return _nodes.GetValueOrDefault(sensorId);
        }

        public List<MeshNode> GetNeighbours(int sensorId)
        {
            if (!_adjacencyList.TryGetValue(
                sensorId, out var connections))
            {
                return new List<MeshNode>();
            }

            return connections
                .Where(e => e.IsActive)
                .Select(e =>
                    e.SourceSensorId == sensorId
                        ? e.DestinationSensorId
                        : e.SourceSensorId)
                .Where(id => _nodes.ContainsKey(id))
                .Select(id => _nodes[id])
                .ToList();
        }

        public List<MeshEdge> GetConnections(int sensorId)
        {
            if (!_adjacencyList.TryGetValue(
                sensorId, out var connections))
            {
                return new List<MeshEdge>();
            }

            return connections
                .Where(e => e.IsActive)
                .ToList();
        }


        public async Task<List<MeshNode>> BreadthFirstSearchAsync(
            int startSensorId)
        {
            return await ExecuteWithGraphLockAsync(
                () => BreadthFirstSearch(startSensorId));
        }

        public List<MeshNode> BreadthFirstSearch(int startSensorId)
        {
            var result = new List<MeshNode>();

            // Return an empty list if the starting sensor
            // does not exist in the network.
            if (!_nodes.ContainsKey(startSensorId))
            {
                return result;
            }

            // Queue controls the order in which
            // sensors are explored.
            var queue = new Queue<int>();

            // HashSet prevents visiting a sensor twice.
            var visited = new HashSet<int>();

            queue.Enqueue(startSensorId);
            visited.Add(startSensorId);

            while (queue.Count > 0)
            {
                int currentSensorId = queue.Dequeue();

                var currentNode = _nodes[currentSensorId];

                // Disconnected sensors cannot relay traffic.
                if (currentNode.Status == SensorStatus.Disconnected)
                {
                    continue;
                }

                result.Add(currentNode);

                // Find all active neighbouring connections.
                var neighbours = GetNeighbours(currentSensorId)
                    .OrderBy(n => n.SensorId);

                foreach (var neighbour in neighbours)
                {
                    if (neighbour.Status == SensorStatus.Disconnected)
                    {
                        continue;
                    }

                    if (visited.Add(neighbour.SensorId))
                    {
                        queue.Enqueue(neighbour.SensorId);
                    }
                }
            }

            return result;
        }




        public async Task<List<MeshEdge>> CalculateMinimumSpanningTreeAsync()
        {
            return await ExecuteWithGraphLockAsync(
                () => CalculateMinimumSpanningTree());
        }



        public async Task<(
            List<MeshEdge> Edges,
            int? GatewaySensorId,
            int ReachableCount,
            double TotalWeight,
            bool IsComplete)>
            GetMinimumSpanningTreeResultsAsync()
        {
            await _graphLock.WaitAsync();

            try
            {
                // Calculate MST while holding the graph lock.
                var mstEdges = CalculateMinimumSpanningTree();

                // Identify the gateway.
                var gateway = _nodes.Values
                    .FirstOrDefault(n => n.IsGateway);

                // Calculate reachable sensors using the
                // existing synchronous BFS implementation.
                int reachableCount = gateway == null
                    ? 0
                    : BreadthFirstSearch(gateway.SensorId).Count;

                // Calculate total connection weight.
                double totalWeight =
                    mstEdges.Sum(edge => edge.Weight);

                // Copy the MST edges so the response
                // does not expose mutable graph objects.
                var edgeCopies = mstEdges
                    .Select(e => new MeshEdge
                    {
                        SourceSensorId = e.SourceSensorId,
                        DestinationSensorId = e.DestinationSensorId,
                        Weight = e.Weight,
                        IsActive = e.IsActive,
                        IsInMinimumSpanningTree =
                            e.IsInMinimumSpanningTree
                    })
                    .ToList();

                bool isComplete =
                    reachableCount > 0 &&
                    edgeCopies.Count == reachableCount - 1;

                return (
                    edgeCopies,
                    gateway?.SensorId,
                    reachableCount,
                    totalWeight,
                    isComplete);
            }
            finally
            {
                _graphLock.Release();
            }
        }

        public List<MeshEdge> CalculateMinimumSpanningTree()
        {
            var minimumSpanningTree = new List<MeshEdge>();

            // Clear previous MST selections.
            foreach (var edge in _edges)
            {
                edge.IsInMinimumSpanningTree = false;
            }

            if (_nodes.Count == 0)
            {
                return minimumSpanningTree;
            }

            // Start from the gateway.
            var gateway = _nodes.Values
                .FirstOrDefault(n => n.IsGateway);

            if (gateway == null ||
                gateway.Status == SensorStatus.Disconnected)
            {
                return minimumSpanningTree;
            }

            var visited = new HashSet<int>();

            // Prioritise links by their connection cost.
            var priorityQueue =
                new PriorityQueue<MeshEdge, double>();

            visited.Add(gateway.SensorId);

            // Add the gateway's available connections.
            foreach (var edge in GetConnections(gateway.SensorId))
            {
                priorityQueue.Enqueue(edge, edge.Weight);
            }

            while (priorityQueue.Count > 0)
            {
                var edge = priorityQueue.Dequeue();

                if (!edge.IsActive)
                {
                    continue;
                }

                int sourceId = edge.SourceSensorId;
                int destinationId = edge.DestinationSensorId;

                bool sourceVisited = visited.Contains(sourceId);
                bool destinationVisited =
                    visited.Contains(destinationId);

                // Skip connections that would create a cycle.
                if (sourceVisited == destinationVisited)
                {
                    continue;
                }

                int nextSensorId = sourceVisited
                    ? destinationId
                    : sourceId;

                if (_nodes[nextSensorId].Status ==
                    SensorStatus.Disconnected)
                {
                    continue;
                }

                // Select this connection for the MST.
                edge.IsInMinimumSpanningTree = true;
                minimumSpanningTree.Add(edge);

                visited.Add(nextSensorId);

                // Explore the newly connected sensor's links.
                foreach (var nextEdge in GetConnections(nextSensorId))
                {
                    int neighbourId =
                        nextEdge.SourceSensorId == nextSensorId
                            ? nextEdge.DestinationSensorId
                            : nextEdge.SourceSensorId;

                    if (!visited.Contains(neighbourId))
                    {
                        priorityQueue.Enqueue(
                            nextEdge,
                            nextEdge.Weight);
                    }
                }
            }

            return minimumSpanningTree;
        }


        public async Task<bool> DisableConnectionAsync(
            int sourceSensorId,
            int destinationSensorId)
        {
            return await ExecuteWithGraphLockAsync(
                () => DisableConnection(
                    sourceSensorId,
                    destinationSensorId));
        }


        public bool DisableConnection(
            int sourceSensorId,
            int destinationSensorId)
        {
            var edge = _edges.FirstOrDefault(e =>
                (e.SourceSensorId == sourceSensorId &&
                 e.DestinationSensorId == destinationSensorId) ||
                (e.SourceSensorId == destinationSensorId &&
                 e.DestinationSensorId == sourceSensorId));

            if (edge == null || !edge.IsActive)
            {
                return false;
            }

            edge.IsActive = false;
            edge.IsInMinimumSpanningTree = false;

            // Recalculate using the remaining active links.
            CalculateMinimumSpanningTree();

            return true;
        }

        public async Task<bool> RestoreConnectionAsync(
            int sourceSensorId,
            int destinationSensorId)
        {
            return await ExecuteWithGraphLockAsync(
                () => RestoreConnection(
                    sourceSensorId,
                    destinationSensorId));
        }

        public bool RestoreConnection(
            int sourceSensorId,
            int destinationSensorId)
        {
            var edge = _edges.FirstOrDefault(e =>
                (e.SourceSensorId == sourceSensorId &&
                 e.DestinationSensorId == destinationSensorId) ||
                (e.SourceSensorId == destinationSensorId &&
                 e.DestinationSensorId == sourceSensorId));

            if (edge == null || edge.IsActive)
            {
                return false;
            }

            edge.IsActive = true;

            CalculateMinimumSpanningTree();

            return true;
        }

        public async Task<List<MeshNode>> GetUnreachableSensorsAsync()
        {
            return await ExecuteWithGraphLockAsync(
                () => GetUnreachableSensors()
                    .Select(n => new MeshNode
                    {
                        SensorId = n.SensorId,
                        MacAddress = n.MacAddress,
                        DeviceName = n.DeviceName,
                        Zone = n.Zone,
                        Status = n.Status,
                        IsGateway = n.IsGateway
                    })
                    .ToList());
        }

        public List<MeshNode> GetUnreachableSensors()
        {
            var gateway = _nodes.Values
                .FirstOrDefault(n => n.IsGateway);

            if (gateway == null)
            {
                return _nodes.Values.ToList();
            }

            var reachableIds = BreadthFirstSearch(gateway.SensorId)
                .Select(n => n.SensorId)
                .ToHashSet();

            return _nodes.Values
                .Where(n => !reachableIds.Contains(n.SensorId))
                .OrderBy(n => n.SensorId)
                .ToList();
        }


        public async Task<List<MeshNode>> DepthFirstSearchAsync(
            int startSensorId)
        {
            return await ExecuteWithGraphLockAsync(
                () => DepthFirstSearch(startSensorId));
        }

        public List<MeshNode> DepthFirstSearch(int startSensorId)
        {
            var result = new List<MeshNode>();

            // Check whether the starting sensor exists.
            if (!_nodes.ContainsKey(startSensorId))
            {
                return result;
            }

            // Stack follows Last-In, First-Out (LIFO).
            var stack = new Stack<int>();

            // Prevent visiting the same sensor twice.
            var visited = new HashSet<int>();

            stack.Push(startSensorId);

            while (stack.Count > 0)
            {
                int currentSensorId = stack.Pop();

                // Skip sensors already processed.
                if (!visited.Add(currentSensorId))
                {
                    continue;
                }

                var currentNode = _nodes[currentSensorId];

                // Disconnected sensors cannot relay traffic.
                if (currentNode.Status == SensorStatus.Disconnected)
                {
                    continue;
                }

                result.Add(currentNode);

                // Reverse ordering so smaller sensor IDs
                // are explored first when popped.
                var neighbours = GetNeighbours(currentSensorId)
                    .OrderByDescending(n => n.SensorId);

                foreach (var neighbour in neighbours)
                {
                    if (neighbour.Status != SensorStatus.Disconnected &&
                        !visited.Contains(neighbour.SensorId))
                    {
                        stack.Push(neighbour.SensorId);
                    }
                }
            }

            return result;
        }

        public async Task<List<MeshNode>> FindRouteByMacAddressAsync(
            string sourceMac,
            string destinationMac)
        {
            return await ExecuteWithGraphLockAsync(
                () => FindRouteByMacAddress(
                    sourceMac,
                    destinationMac)
                .Select(n => new MeshNode
                {
                    SensorId = n.SensorId,
                    MacAddress = n.MacAddress,
                    DeviceName = n.DeviceName,
                    Zone = n.Zone,
                    Status = n.Status,
                    IsGateway = n.IsGateway
                })
                .ToList());
        }


        public List<MeshNode> FindRouteByMacAddress(
            string sourceMacAddress,
            string destinationMacAddress)
        {
            var route = new List<MeshNode>();

            // Find sensors by MAC address.
            var source = _nodes.Values.FirstOrDefault(n =>
                string.Equals(
                    n.MacAddress,
                    sourceMacAddress,
                    StringComparison.OrdinalIgnoreCase));

            var destination = _nodes.Values.FirstOrDefault(n =>
                string.Equals(
                    n.MacAddress,
                    destinationMacAddress,
                    StringComparison.OrdinalIgnoreCase));

            if (source == null || destination == null)
            {
                return route;
            }

            // Disconnected devices cannot participate in routing.
            if (source.Status == SensorStatus.Disconnected ||
                destination.Status == SensorStatus.Disconnected)
            {
                return route;
            }

            var queue = new Queue<int>();
            var visited = new HashSet<int>();

            // Child sensor ID -> parent sensor ID.
            var parents = new Dictionary<int, int>();

            queue.Enqueue(source.SensorId);
            visited.Add(source.SensorId);

            bool destinationFound = false;

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();

                if (currentId == destination.SensorId)
                {
                    destinationFound = true;
                    break;
                }

                foreach (var neighbour in GetNeighbours(currentId)
                    .OrderBy(n => n.SensorId))
                {
                    if (neighbour.Status == SensorStatus.Disconnected)
                    {
                        continue;
                    }

                    if (visited.Add(neighbour.SensorId))
                    {
                        parents[neighbour.SensorId] = currentId;
                        queue.Enqueue(neighbour.SensorId);
                    }
                }
            }

            if (!destinationFound)
            {
                return route;
            }

            // Reconstruct the route backwards.
            int current = destination.SensorId;

            while (current != source.SensorId)
            {
                route.Add(_nodes[current]);
                current = parents[current];
            }

            route.Add(source);

            // Reverse to show source -> destination.
            route.Reverse();

            return route;
        }


        public async Task<(
            bool Success,
            List<MeshEdge> MinimumSpanningTree,
            List<MeshNode> UnreachableSensors)>
            ChangeConnectionAndGetResultsAsync(
                int sourceSensorId,
                int destinationSensorId,
                bool restore)
        {
            await _graphLock.WaitAsync();

            try
            {
                bool success = restore
                    ? RestoreConnection(
                        sourceSensorId, destinationSensorId)
                    : DisableConnection(
                        sourceSensorId, destinationSensorId);

                if (!success)
                {
                    return (
                        false,
                        new List<MeshEdge>(),
                        new List<MeshNode>());
                }

                var mst = CalculateMinimumSpanningTree()
                    .Select(e => new MeshEdge
                    {
                        SourceSensorId = e.SourceSensorId,
                        DestinationSensorId = e.DestinationSensorId,
                        Weight = e.Weight,
                        IsActive = e.IsActive,
                        IsInMinimumSpanningTree =
                            e.IsInMinimumSpanningTree
                    })
                    .ToList();

                var unreachable = GetUnreachableSensors()
                    .Select(n => new MeshNode
                    {
                        SensorId = n.SensorId,
                        MacAddress = n.MacAddress,
                        DeviceName = n.DeviceName,
                        Zone = n.Zone,
                        Status = n.Status,
                        IsGateway = n.IsGateway
                    })
                    .ToList();

                return (true, mst, unreachable);
            }
            finally
            {
                _graphLock.Release();
            }
        }

    }

}