using System.Text.Json;

namespace SmartX.Client.Models
{
    public class NetworkTopologyViewModel
    {
        public List<MeshNodeViewModel> Nodes { get; set; }
            = new();

        public List<MeshEdgeViewModel> Edges { get; set; }
            = new();

        public List<MeshEdgeViewModel> MinimumSpanningTree
        { get; set; } = new();

        public List<MeshNodeViewModel> UnreachableSensors
        { get; set; } = new();

        public int TotalSensors => Nodes.Count;

        public int ActiveConnections =>
            Edges.Count(e => e.IsActive);

        public int FailedConnections =>
            Edges.Count(e => !e.IsActive);

        public double TotalMstWeight =>
            MinimumSpanningTree.Sum(e => e.Weight);

        public string SourceMac { get; set; }
            = string.Empty;

        public string DestinationMac { get; set; }
            = string.Empty;

        public bool? RouteFound { get; set; }

        public List<MeshNodeViewModel> Route { get; set; }
            = new();

        public string? ErrorMessage { get; set; }
    }

    public class MeshNodeViewModel
    {
        public int SensorId { get; set; }

        public string MacAddress { get; set; }
            = string.Empty;

        public string DeviceName { get; set; }
            = string.Empty;

        public string Zone { get; set; }
            = string.Empty;

        [System.Text.Json.Serialization.JsonConverter(
            typeof(SensorStatusJsonConverter))]
        public string Status { get; set; } = string.Empty;

        public bool IsGateway { get; set; }
    }

    public class MeshEdgeViewModel
    {
        public int SourceSensorId { get; set; }

        public int DestinationSensorId { get; set; }

        public double Weight { get; set; }

        public bool IsActive { get; set; }

        public bool IsInMinimumSpanningTree { get; set; }
    }


    public class SensorStatusJsonConverter
        : System.Text.Json.Serialization.JsonConverter<string>
    {
        public override string Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32() switch
                {
                    0 => "Connected",
                    1 => "Warning",
                    2 => "Disconnected",
                    _ => "Unknown"
                };
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? "Unknown";
            }

            return "Unknown";
        }

        public override void Write(
            Utf8JsonWriter writer,
            string value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

}
