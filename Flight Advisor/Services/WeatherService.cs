// Services/WeatherService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using FlightAdvisor.Models;

namespace FlightAdvisor.Services
{
    public class WeatherService
    {
        private readonly IWeatherApi _weatherApi;
        private const string NOAA_BASE_URL = "https://aviationweather.gov";

        public WeatherService()
        {
            _weatherApi = RestService.For<IWeatherApi>(NOAA_BASE_URL);
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
                throw new WeatherServiceException($"Failed to fetch METAR for {icao}", ex);
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
                throw new WeatherServiceException($"Failed to fetch TAF for {icao}", ex);
            }
        }

        /// <summary>
        /// Fetch runway information from METAR data
        /// </summary>
        public async Task<List<string>> GetRunwaysAsync(string icao)
        {
            try
            {
                // NOAA doesn't have direct runway API, parse from common patterns
                // For MVP, we'll extract from known patterns or return empty
                var metar = await GetMetarAsync(icao);

                // Parse runways from METAR remarks if available
                // This is a simplified version - will be enhanced
                var runways = new List<string>();

                // Common runway patterns for US airports
                // This should be replaced with actual FAA API when available
                if (!string.IsNullOrEmpty(icao))
                {
                    // Placeholder: Return common runway configurations
                    // In production, this would query FAA ADIP API
                    runways.Add("Auto-Selected");
                }

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
            // Wind angle relative to runway
            var angleDiff = Math.Abs(windDirection - runwayHeading);
            if (angleDiff > 180)
                angleDiff = 360 - angleDiff;

            // Crosswind component = wind speed × sin(angle)
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

            // Headwind component = wind speed × cos(angle)
            var headwind = windSpeed * Math.Cos(angleDiff * Math.PI / 180);
            return (int)Math.Round(headwind);
        }

        /// <summary>
        /// Calculate density altitude
        /// </summary>
        public int CalculateDensityAltitude(int elevation, double temperature, double altimeter)
        {
            // Standard pressure: 29.92 inHg
            // Pressure altitude = elevation + (29.92 - altimeter) × 1000
            var pressureAltitude = elevation + ((29.92 - altimeter) * 1000);

            // Density altitude = pressure altitude + (120 × (temp - standard temp))
            // Standard temp at sea level = 15°C, lapse rate = 2°C per 1000ft
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

            // Handle "10+" or "10" formats
            var cleanVis = visibilityString.Replace("+", "").Trim();

            if (double.TryParse(cleanVis, out var visibility))
            {
                // NOAA returns visibility in statute miles, convert to meters
                return visibility * 1609.34; // 1 mile = 1609.34 meters
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

            // Check for common hazards
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
    /// Custom exception for weather service errors
    /// </summary>
    public class WeatherServiceException : Exception
    {
        public WeatherServiceException(string message) : base(message) { }
        public WeatherServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}