// Models/AircraftModels.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlightAdvisor.Models
{
    /// <summary>
    /// Aircraft Database Root
    /// </summary>
    public class AircraftDatabase
    {
        [JsonPropertyName("trainers")]
        public List<Aircraft> Trainers { get; set; }

        [JsonPropertyName("gliders")]
        public List<Aircraft> Gliders { get; set; }

        public List<Aircraft> GetAllAircraft()
        {
            var all = new List<Aircraft>();
            if (Trainers != null) all.AddRange(Trainers);
            if (Gliders != null) all.AddRange(Gliders);
            return all;
        }
    }

    /// <summary>
    /// Aircraft Model
    /// </summary>
    public class Aircraft
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("maxCrosswind")]
        public int MaxCrosswind { get; set; }

        [JsonPropertyName("maxWindSpeed")]
        public int MaxWindSpeed { get; set; }

        [JsonPropertyName("maxDemonstrated")]
        public int MaxDemonstrated { get; set; }

        [JsonPropertyName("stallSpeedClean")]
        public int? StallSpeedClean { get; set; }

        [JsonPropertyName("stallSpeedLanding")]
        public int? StallSpeedLanding { get; set; }

        [JsonPropertyName("stallSpeed")]
        public int? StallSpeed { get; set; }

        [JsonPropertyName("maxOperatingAltitude")]
        public int MaxOperatingAltitude { get; set; }

        [JsonPropertyName("maxTakeoffWeight")]
        public int MaxTakeoffWeight { get; set; }

        [JsonPropertyName("minRunwayLength")]
        public int MinRunwayLength { get; set; }

        [JsonPropertyName("maxWinchSpeed")]
        public int? MaxWinchSpeed { get; set; }

        [JsonPropertyName("minVisibility")]
        public int MinVisibility { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; }

        public override string ToString() => $"{Name} ({Id})";
    }

    /// <summary>
    /// Flight Information
    /// </summary>
    public class FlightInfo
    {
        public string FlightType { get; set; }
        public Aircraft SelectedAircraft { get; set; }
        public string DepartureIcao { get; set; }
        public string ArrivalIcao { get; set; }
        public string AlternateIcao { get; set; }
        public int TakeoffWeight { get; set; }
        public int LandingWeight { get; set; }
        public int SoulsOnBoard { get; set; }
        public double FuelPlanned { get; set; }
        public string Route { get; set; }
        public string SelectedRunway { get; set; }
    }

    /// <summary>
    /// Flight Types
    /// </summary>
    public enum FlightType
    {
        FlightLesson,
        Gliding,
        Recreational,
        DiscoveryFlight,
        JustLooking
    }
}