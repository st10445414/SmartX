
namespace SmartX.Api.Models
{
    public class MeshEdge
    {
        // The sensor where the connection starts.
        public int SourceSensorId { get; set; }

        // The sensor where the connection ends.
        public int DestinationSensorId { get; set; }

        // Simulated connection cost.
        // Lower values represent better connections.
        public double Weight { get; set; }

        // Whether this connection can currently be used.
        public bool IsActive { get; set; } = true;

        // Indicates whether this connection was
        // selected by the minimum spanning tree.
        public bool IsInMinimumSpanningTree { get; set; }
            = false;
    }
}
