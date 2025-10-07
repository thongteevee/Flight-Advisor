// Models/WeatherModels.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FlightAdvisor.Services;

namespace FlightAdvisor.Models
{
    /// <summary>
    /// METAR Weather Data Model - ALL fields nullable to handle API variations
    /// </summary>
    public class MetarData
    {
        [JsonPropertyName("metar_id")]
        public int? MetarId { get; set; }

        [JsonPropertyName("icaoId")]
        public string IcaoId { get; set; }

        [JsonPropertyName("receiptTime")]
        public DateTime? ReceiptTime { get; set; }

        [JsonPropertyName("obsTime")]
        public long? ObservationTimeUnix { get; set; }

        [JsonPropertyName("reportTime")]
        public DateTime? ReportTime { get; set; }

        // Computed property for ObservationTime
        [JsonIgnore]
        public DateTime ObservationTime => ObservationTimeUnix.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(ObservationTimeUnix.Value).DateTime
            : (ReportTime ?? DateTime.MinValue);

        [JsonPropertyName("temp")]
        public double? Temperature { get; set; }

        [JsonPropertyName("dewp")]
        public double? DewPoint { get; set; }

        [JsonPropertyName("wdir")]
        public int? WindDirection { get; set; }

        [JsonPropertyName("wspd")]
        public int? WindSpeed { get; set; }

        [JsonPropertyName("wgst")]
        public int? WindGust { get; set; }

        [JsonPropertyName("visib")]
        [JsonConverter(typeof(FlexibleVisibilityConverter))]
        public string Visibility { get; set; }

        [JsonPropertyName("altim")]
        public double? Altimeter { get; set; }

        [JsonPropertyName("slp")]
        public double? SeaLevelPressure { get; set; }

        [JsonPropertyName("qcField")]
        public int? QcField { get; set; }

        [JsonPropertyName("wxString")]
        public string WeatherString { get; set; }

        [JsonPropertyName("presTend")]
        public int? PressureTendency { get; set; }

        [JsonPropertyName("maxT")]
        public double? MaxTemperature { get; set; }

        [JsonPropertyName("minT")]
        public double? MinTemperature { get; set; }

        [JsonPropertyName("maxT24")]
        public double? MaxTemperature24h { get; set; }

        [JsonPropertyName("minT24")]
        public double? MinTemperature24h { get; set; }

        [JsonPropertyName("precip")]
        public double? Precipitation { get; set; }

        [JsonPropertyName("pcp3hr")]
        public double? Precipitation3h { get; set; }

        [JsonPropertyName("pcp6hr")]
        public double? Precipitation6h { get; set; }

        [JsonPropertyName("pcp24hr")]
        public double? Precipitation24h { get; set; }

        [JsonPropertyName("snow")]
        public double? Snow { get; set; }

        [JsonPropertyName("vertVis")]
        public int? VerticalVisibility { get; set; }

        [JsonPropertyName("metarType")]
        public string MetarType { get; set; }

        [JsonPropertyName("rawOb")]
        public string RawObservation { get; set; }

        [JsonPropertyName("mostRecent")]
        public int? MostRecent { get; set; }

        [JsonPropertyName("lat")]
        public double? Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double? Longitude { get; set; }

        [JsonPropertyName("elev")]
        public int? Elevation { get; set; }

        [JsonPropertyName("prior")]
        public int? Prior { get; set; }

        [JsonPropertyName("name")]
        public string AirportName { get; set; }

        [JsonPropertyName("clouds")]
        public List<CloudLayer> Clouds { get; set; }
    }

    /// <summary>
    /// Cloud Layer Information
    /// </summary>
    public class CloudLayer
    {
        [JsonPropertyName("cover")]
        public string Cover { get; set; }

        [JsonPropertyName("base")]
        public int? Base { get; set; }
    }

    /// <summary>
    /// TAF (Terminal Aerodrome Forecast) Data Model
    /// </summary>
    public class TafData
    {
        [JsonPropertyName("taf_id")]
        public int? TafId { get; set; }

        [JsonPropertyName("icaoId")]
        public string IcaoId { get; set; }

        [JsonPropertyName("dbPopTime")]
        public DateTime? DbPopTime { get; set; }

        [JsonPropertyName("bulletinTime")]
        public DateTime? BulletinTime { get; set; }

        [JsonPropertyName("issueTime")]
        public DateTime? IssueTime { get; set; }

        [JsonPropertyName("validTimeFrom")]
        public DateTime? ValidTimeFrom { get; set; }

        [JsonPropertyName("validTimeTo")]
        public DateTime? ValidTimeTo { get; set; }

        [JsonPropertyName("rawTAF")]
        public string RawTaf { get; set; }

        [JsonPropertyName("mostRecent")]
        public int? MostRecent { get; set; }

        [JsonPropertyName("lat")]
        public double? Latitude { get; set; }

        [JsonPropertyName("lon")]
        public double? Longitude { get; set; }

        [JsonPropertyName("elev")]
        public int? Elevation { get; set; }

        [JsonPropertyName("fcsts")]
        public List<TafForecast> Forecasts { get; set; }
    }

    /// <summary>
    /// TAF Forecast Period
    /// </summary>
    public class TafForecast
    {
        [JsonPropertyName("fcst_time_from")]
        public DateTime? ForecastTimeFrom { get; set; }

        [JsonPropertyName("fcst_time_to")]
        public DateTime? ForecastTimeTo { get; set; }

        [JsonPropertyName("timeBecoming")]
        public DateTime? TimeBecoming { get; set; }

        [JsonPropertyName("probability")]
        public int? Probability { get; set; }

        [JsonPropertyName("change_indicator")]
        public string ChangeIndicator { get; set; }

        [JsonPropertyName("wdir")]
        public int? WindDirection { get; set; }

        [JsonPropertyName("wspd")]
        public int? WindSpeed { get; set; }

        [JsonPropertyName("wgst")]
        public int? WindGust { get; set; }

        [JsonPropertyName("wshearDir")]
        public int? WindShearDirection { get; set; }

        [JsonPropertyName("wshearSpd")]
        public int? WindShearSpeed { get; set; }

        [JsonPropertyName("wshearHgt")]
        public int? WindShearHeight { get; set; }

        [JsonPropertyName("visib")]
        public string Visibility { get; set; }

        [JsonPropertyName("altim")]
        public double? Altimeter { get; set; }

        [JsonPropertyName("vertVis")]
        public int? VerticalVisibility { get; set; }

        [JsonPropertyName("wxString")]
        public string WeatherString { get; set; }

        [JsonPropertyName("temp")]
        public double? Temperature { get; set; }

        [JsonPropertyName("sky")]
        public List<CloudLayer> Sky { get; set; }
    }

    /// <summary>
    /// Airport Runway Information
    /// </summary>
    public class RunwayInfo
    {
        public string RunwayId { get; set; }
        public int Heading { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public string Surface { get; set; }
    }
}