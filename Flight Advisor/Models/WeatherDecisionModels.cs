// Models/WeatherDecisionModels.cs
using System;
using System.Collections.Generic;

namespace FlightAdvisor.Models
{
    /// <summary>
    /// Go/No-Go Decision Result
    /// </summary>
    public class FlightDecision
    {
        public DecisionType Decision { get; set; }
        public string Summary { get; set; }
        public List<WeatherHazard> Hazards { get; set; } = new List<WeatherHazard>();
        public List<string> Cautions { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public WeatherSummary WeatherSummary { get; set; }
        public DateTime EvaluatedAt { get; set; } = DateTime.Now;

        public string GetDecisionIcon()
        {
            return Decision switch
            {
                DecisionType.Go => "✅",
                DecisionType.Caution => "⚠️",
                DecisionType.NoGo => "❌",
                _ => "ℹ️"
            };
        }

        public string GetDecisionColor()
        {
            return Decision switch
            {
                DecisionType.Go => "#22c55e",
                DecisionType.Caution => "#f59e0b",
                DecisionType.NoGo => "#ef4444",
                _ => "#6b7280"
            };
        }
    }

    /// <summary>
    /// Decision Types
    /// </summary>
    public enum DecisionType
    {
        Go,
        Caution,
        NoGo,
        InformationOnly
    }

    /// <summary>
    /// Weather Hazard
    /// </summary>
    public class WeatherHazard
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public HazardSeverity Severity { get; set; }
        public string Value { get; set; }
        public string Threshold { get; set; }
        public string QuickTip { get; set; }
    }

    /// <summary>
    /// Hazard Severity
    /// </summary>
    public enum HazardSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Weather Summary for Display
    /// </summary>
    public class WeatherSummary
    {
        public string AirportIcao { get; set; }
        public string AirportName { get; set; }
        public DateTime ObservationTime { get; set; }
        public double? Temperature { get; set; }
        public double? DewPoint { get; set; }
        public int? WindDirection { get; set; }
        public int? WindSpeed { get; set; }
        public int? WindGust { get; set; }
        public string Visibility { get; set; }
        public double? Altimeter { get; set; }
        public int Elevation { get; set; }
        public string WeatherConditions { get; set; }
        public List<string> CloudLayers { get; set; } = new List<string>();
        public string RawMetar { get; set; }
        public string RawTaf { get; set; }

        // Calculated values
        public int? CrosswindComponent { get; set; }
        public int? HeadwindComponent { get; set; }
        public int? DensityAltitude { get; set; }
        public bool HasPrecipitation { get; set; }
        public bool HasIcing { get; set; }
        public bool HasThunderstorms { get; set; }
    }
}