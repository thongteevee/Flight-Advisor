// Services/WeatherService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FlightAdvisor.Models;

namespace FlightAdvisor.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private const string NOAA_BASE_URL = "https://aviationweather.gov";

        public WeatherService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(NOAA_BASE_URL)
            };
        }

        public async Task<MetarData> GetMetarAsync(string icao)
        {
            try
            {
                var url = $"/api/data/metar?ids={icao.ToUpper()}&format=json&taf=false";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<MetarData>>(json);
                return result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new WeatherServiceException($"Failed to fetch METAR for {icao}: {ex.Message}", ex);
            }
        }

        public async Task<TafData> GetTafAsync(string icao)
        {
            try
            {
                var url = $"/api/data/taf?ids={icao.ToUpper()}&format=json";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<TafData>>(json);
                return result?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new WeatherServiceException($"Failed to fetch TAF for {icao}: {ex.Message}", ex);
            }
        }

        // Keep all other methods (CalculateCrosswind, ParseVisibility, etc.) exactly as they are

        public async Task<List<string>> GetRunwaysAsync(string icao)
        {
            var runways = new List<string> { "Auto-Selected" };
            return await Task.FromResult(runways);
        }

        public int CalculateCrosswind(int windDirection, int windSpeed, int runwayHeading)
        {
            var angleDiff = Math.Abs(windDirection - runwayHeading);
            if (angleDiff > 180)
                angleDiff = 360 - angleDiff;
            var crosswind = windSpeed * Math.Sin(angleDiff * Math.PI / 180);
            return (int)Math.Round(Math.Abs(crosswind));
        }

        public int CalculateHeadwind(int windDirection, int windSpeed, int runwayHeading)
        {
            var angleDiff = Math.Abs(windDirection - runwayHeading);
            if (angleDiff > 180)
                angleDiff = 360 - angleDiff;
            var headwind = windSpeed * Math.Cos(angleDiff * Math.PI / 180);
            return (int)Math.Round(headwind);
        }

        public int CalculateDensityAltitude(int elevation, double temperature, double altimeter)
        {
            var pressureAltitude = elevation + ((29.92 - altimeter) * 1000);
            var standardTemp = 15 - (elevation / 1000.0 * 2);
            var densityAltitude = pressureAltitude + (120 * (temperature - standardTemp));
            return (int)Math.Round(densityAltitude);
        }

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

    public class WeatherServiceException : Exception
    {
        public WeatherServiceException(string message) : base(message) { }
        public WeatherServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}