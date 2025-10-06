// Services/WeatherService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Refit;
using FlightAdvisor.Models;

namespace FlightAdvisor.Services
{
    public class WeatherService
    {
        private readonly IWeatherApi _weatherApi;
        private const string NOAA_BASE_URL = "https://aviationweather.gov";

        // Custom JSON options with flexible DateTime handling
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new FlexibleDateTimeConverter() }
        };

        public WeatherService()
        {
            var refitSettings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(JsonOptions)
            };

            _weatherApi = RestService.For<IWeatherApi>(NOAA_BASE_URL, refitSettings);
        }

        /// <summary>
        /// Fetch METAR data for an airport
        /// </summary>
        public async Task<MetarData> GetMetarAsync(string icao)
        {
            try
            {
                var result = await _weatherApi.GetMetarAsync(icao.ToUpper(), hours: 0);
                return result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new WeatherServiceException($"Failed to fetch METAR for {icao}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Fetch TAF data for an airport
        /// </summary>
        public async Task<TafData> GetTafAsync(string icao)
        {
            try
            {
                var result = await _weatherApi.GetTafAsync(icao.ToUpper(), hours: 0);
                return result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new WeatherServiceException($"Failed to fetch TAF for {icao}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Fetch runway information from METAR data
        /// </summary>
        public async Task<List<string>> GetRunwaysAsync(string icao)
        {
            try
            {
                var runways = new List<string>();
                runways.Add("Auto-Selected");
                return runways;
            }
            catch (Exception ex)
            {
                throw new WeatherServiceException($"Failed to fetch runways for {icao}", ex);
            }
        }

        /// <summary>
        /// Calculate crosswind component
        /// </summary>
        public int CalculateCrosswind(int windDirection, int windSpeed, int runwayHeading)
        {
            var angleDiff = Math.Abs(windDirection - runwayHeading);
            if (angleDiff > 180)
                angleDiff = 360 - angleDiff;

            var crosswind = windSpeed * Math.Sin(angleDiff * Math.PI / 180);
            return (int)Math.Round(Math.Abs(crosswind));
        }

        /// <summary>
        /// Calculate headwind/tailwind component
        /// </summary>
        public int CalculateHeadwind(int windDirection, int windSpeed, int runwayHeading)
        {
            var angleDiff = Math.Abs(windDirection - runwayHeading);
            if (angleDiff > 180)
                angleDiff = 360 - angleDiff;

            var headwind = windSpeed * Math.Cos(angleDiff * Math.PI / 180);
            return (int)Math.Round(headwind);
        }

        /// <summary>
        /// Calculate density altitude
        /// </summary>
        public int CalculateDensityAltitude(int elevation, double temperature, double altimeter)
        {
            var pressureAltitude = elevation + ((29.92 - altimeter) * 1000);
            var standardTemp = 15 - (elevation / 1000.0 * 2);
            var densityAltitude = pressureAltitude + (120 * (temperature - standardTemp));
            return (int)Math.Round(densityAltitude);
        }

        /// <summary>
        /// Parse visibility from METAR string
        /// </summary>
        public double ParseVisibility(string visibilityString)
        {
            if (string.IsNullOrEmpty(visibilityString))
                return 0;

            var cleanVis = visibilityString.Replace("+", "").Trim();

            if (double.TryParse(cleanVis, out var visibility))
            {
                return visibility * 1609.34;
            }

            return 0;
        }

        /// <summary>
        /// Check for hazardous weather conditions
        /// </summary>
        public List<string> ParseWeatherHazards(string wxString)
        {
            var hazards = new List<string>();

            if (string.IsNullOrEmpty(wxString))
                return hazards;

            var wx = wxString.ToUpper();

            if (wx.Contains("TS")) hazards.Add("Thunderstorms");
            if (wx.Contains("SN")) hazards.Add("Snow");
            if (wx.Contains("FZRA")) hazards.Add("Freezing Rain");
            if (wx.Contains("FG")) hazards.Add("Fog");
            if (wx.Contains("BR")) hazards.Add("Mist");
            if (wx.Contains("RA")) hazards.Add("Rain");
            if (wx.Contains("DZ")) hazards.Add("Drizzle");
            if (wx.Contains("SH")) hazards.Add("Showers");
            if (wx.Contains("GR")) hazards.Add("Hail");
            if (wx.Contains("IC")) hazards.Add("Ice Crystals");

            return hazards;
        }
    }

    /// <summary>
    /// Custom DateTime converter that handles multiple formats including Unix timestamps
    /// </summary>
    public class FlexibleDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly string[] DateFormats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-dd HH:mm:ss.fff",
            "MM/dd/yyyy HH:mm:ss",
            "yyyy-MM-dd"
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Check if it's a number (Unix timestamp)
            if (reader.TokenType == JsonTokenType.Number)
            {
                var timestamp = reader.GetInt64();
                // Convert Unix timestamp to DateTime (assuming seconds since epoch)
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }

            // Otherwise treat as string
            if (reader.TokenType == JsonTokenType.String)
            {
                var dateString = reader.GetString();

                if (string.IsNullOrEmpty(dateString))
                    return DateTime.MinValue;

                // Try parsing with multiple formats
                foreach (var format in DateFormats)
                {
                    if (DateTime.TryParseExact(dateString, format,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }

                // Fallback to default parsing
                if (DateTime.TryParse(dateString, out var fallbackResult))
                {
                    return fallbackResult;
                }
            }

            // If all else fails, return a default value
            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
        }
    }

    /// <summary>
    /// Custom exception for weather service errors
    /// </summary>
    public class WeatherServiceException : Exception
    {
        public WeatherServiceException(string message) : base(message) { }
        public WeatherServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}