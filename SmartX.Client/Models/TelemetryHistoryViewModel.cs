
using System.Text.Json;

namespace SmartX.Client.Models
{
    public class TelemetryHistoryViewModel
    {
        // Sensor selected by the user.
        public int SensorId { get; set; }

        // Date range selected by the user.
        public DateTime StartDate { get; set; }
            = DateTime.Today.AddDays(-7);

        public DateTime EndDate { get; set; }
            = DateTime.Today.AddDays(1);

        // Indicates whether a search has been performed.
        public bool HasSearched { get; set; }

        // Algorithm used to retrieve readings.
        public string Algorithm { get; set; }
            = "AVL Tree";

        // Historical telemetry returned by the API.
        public List<JsonElement> Readings { get; set; }
            = new();

        // Total number of returned readings.
        public int TotalReadings => Readings.Count;

        // Display any API or validation errors.
        public string? ErrorMessage { get; set; }
    }
}
