
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeshTopologyController : ControllerBase
    {
        private readonly MeshGraphService _meshGraphService;

        public MeshTopologyController(
            MeshGraphService meshGraphService)
        {
            _meshGraphService = meshGraphService;
        }

        // GET: api/MeshTopology
        // Returns all nodes and network connections.
        [HttpGet]
        public async Task<IActionResult> GetTopology()
        {
            await _meshGraphService.BuildGraphAsync();

            return Ok(new
            {
                Nodes = _meshGraphService.GetNodes(),
                Edges = _meshGraphService.GetEdges()
            });
        }

        // GET: api/MeshTopology/nodes
        // Returns registered mesh devices.
        [HttpGet("nodes")]
        public async Task<IActionResult> GetNodes()
        {
            await _meshGraphService.BuildGraphAsync();

            return Ok(_meshGraphService.GetNodes());
        }

        // GET: api/MeshTopology/neighbours/1
        // Returns devices directly connected to sensor 1.
        [HttpGet("neighbours/{sensorId:int}")]
        public async Task<IActionResult> GetNeighbours(
            int sensorId)
        {
            await _meshGraphService.BuildGraphAsync();

            var sensor = _meshGraphService.GetNode(sensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    Message = "Sensor not found in mesh network."
                });
            }

            return Ok(new
            {
                Sensor = sensor,
                Neighbours =
                    _meshGraphService.GetNeighbours(sensorId)
            });
        }

        // GET: api/MeshTopology/bfs/1
        // Performs Breadth-First Search from sensor 1.
        [HttpGet("bfs/{sensorId:int}")]
        public async Task<IActionResult> RunBfs(
            int sensorId)
        {
            await _meshGraphService.BuildGraphAsync();

            var sensor = _meshGraphService.GetNode(sensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    Message = "Starting sensor not found."
                });
            }

            var reachableSensors =
                await _meshGraphService.BreadthFirstSearchAsync(sensorId);

            return Ok(new
            {
                StartingSensorId = sensorId,
                TraversalAlgorithm = "Breadth-First Search",
                ReachableCount = reachableSensors.Count,
                TraversalOrder = reachableSensors
            });
        }



        // GET: api/MeshTopology/mst
        // Calculates the minimum spanning tree.

        [HttpGet("mst")]
        public async Task<IActionResult> GetMinimumSpanningTree()
        {
            await _meshGraphService.BuildGraphAsync();

            var result =
                await _meshGraphService
                    .GetMinimumSpanningTreeResultsAsync();

            return Ok(new
            {
                Algorithm = "Prim's Minimum Spanning Tree",
                GatewaySensorId = result.GatewaySensorId,
                ReachableSensorCount = result.ReachableCount,
                SelectedEdgeCount = result.Edges.Count,
                TotalWeight = result.TotalWeight,
                IsSpanningTreeComplete = result.IsComplete,
                Connections = result.Edges
            });
        }




        [HttpPost("fail/{sourceId:int}/{destinationId:int}")]
        public async Task<IActionResult> SimulateLinkFailure(
            int sourceId,
            int destinationId)
        {
            await _meshGraphService.BuildGraphAsync();

            var result =
                await _meshGraphService.ChangeConnectionAndGetResultsAsync(
                    sourceId,
                    destinationId,
                    restore: false);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    Message = "Connection not found or already inactive."
                });
            }

            return Ok(new
            {
                Message = "Connection failure simulated.",
                SourceSensorId = sourceId,
                DestinationSensorId = destinationId,
                MinimumSpanningTree = result.MinimumSpanningTree,
                UnreachableSensors = result.UnreachableSensors
            });
        }


        // POST: api/MeshTopology/restore/1/2
        [HttpPost("restore/{sourceId:int}/{destinationId:int}")]
        public async Task<IActionResult> RestoreLink(
            int sourceId,
            int destinationId)
        {
            await _meshGraphService.BuildGraphAsync();

            var result =
                await _meshGraphService.ChangeConnectionAndGetResultsAsync(
                    sourceId,
                    destinationId,
                    restore: true);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    Message = "Connection not found or already active."
                });
            }

            return Ok(new
            {
                Message = "Connection restored successfully.",
                MinimumSpanningTree = result.MinimumSpanningTree,
                UnreachableSensors = result.UnreachableSensors
            });
        }


        // GET: api/MeshTopology/unreachable

        [HttpGet("unreachable")]
        public async Task<IActionResult> GetUnreachable()
        {
            await _meshGraphService.BuildGraphAsync();

            var unreachableSensors =
                await _meshGraphService.GetUnreachableSensorsAsync();

            return Ok(new
            {
                UnreachableSensors = unreachableSensors
            });
        }



        // GET: api/MeshTopology/dfs/1
        // Performs Depth-First Search from a selected sensor.
        [HttpGet("dfs/{sensorId:int}")]
        public async Task<IActionResult> RunDfs(int sensorId)
        {
            await _meshGraphService.BuildGraphAsync();

            var sensor = _meshGraphService.GetNode(sensorId);

            if (sensor == null)
            {
                return NotFound(new
                {
                    Message = "Starting sensor not found."
                });
            }

            var traversal =
                await _meshGraphService.DepthFirstSearchAsync(sensorId);


            return Ok(new
            {
                StartingSensorId = sensorId,
                TraversalAlgorithm = "Depth-First Search",
                ReachableCount = traversal.Count,
                TraversalOrder = traversal
            });
        }


        // GET: api/MeshTopology/route
        // Trace a route using two MAC addresses.
        [HttpGet("route")]
        public async Task<IActionResult> TraceRoute(
            [FromQuery] string sourceMac,
            [FromQuery] string destinationMac)
        {
            await _meshGraphService.BuildGraphAsync();

            if (string.IsNullOrWhiteSpace(sourceMac) ||
                string.IsNullOrWhiteSpace(destinationMac))
            {
                return BadRequest(new
                {
                    Message = "Both MAC addresses are required."
                });
            }

            var nodes = _meshGraphService.GetNodes();

            var sourceExists = nodes.Any(n =>
                string.Equals(n.MacAddress, sourceMac,
                    StringComparison.OrdinalIgnoreCase));

            var destinationExists = nodes.Any(n =>
                string.Equals(n.MacAddress, destinationMac,
                    StringComparison.OrdinalIgnoreCase));

            if (!sourceExists || !destinationExists)
            {
                return NotFound(new
                {
                    Message = "One or both MAC addresses were not found."
                });
            }


            var route = await _meshGraphService.FindRouteByMacAddressAsync(
                sourceMac, destinationMac);


            if (route.Count == 0)
            {
                return Ok(new
                {
                    RouteFound = false,
                    Message = "No active route exists between these sensors.",
                    SourceMac = sourceMac,
                    DestinationMac = destinationMac
                });
            }

            return Ok(new
            {
                RouteFound = true,
                Algorithm = "Breadth-First Search",
                SourceMac = sourceMac,
                DestinationMac = destinationMac,
                HopCount = route.Count - 1,
                Route = route
            });
        }


    }
}
