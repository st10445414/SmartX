
namespace SmartX.Api.Models
{
    public class MeshNode
    {
        public int SensorId { get; set; }

        public string MacAddress { get; set; }
            = string.Empty;

        public string DeviceName { get; set; }
            = string.Empty;

        public string Zone { get; set; }
            = string.Empty;

        public SensorStatus Status { get; set; }
            = SensorStatus.Connected;

        public bool IsGateway { get; set; }
            = false;
    }
}
