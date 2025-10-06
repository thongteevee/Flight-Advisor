// Services/IWeatherApi.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using FlightAdvisor.Models;

namespace FlightAdvisor.Services
{
    /// <summary>
    /// NOAA Aviation Weather API Interface
    /// </summary>
    public interface IWeatherApi
    {
        [Get("/api/data/metar")]
        Task<List<MetarData>> GetMetarAsync(
            [Query] string ids,
            [Query] int hours = 0,
            [Query] string format = "json"
        );

        [Get("/api/data/taf")]
        Task<List<TafData>> GetTafAsync(
            [Query] string ids,
            [Query] int hours = 0,
            [Query] string format = "json"
        );

        [Get("/api/data/airport")]
        Task<List<AirportData>> GetAirportDataAsync(
            [Query] string ids,
            [Query] string format = "json"
        );
    }

    /// <summary>
    /// Airport Data from NOAA (includes runway info)
    /// </summary>
    public class AirportData
    {
        public string IcaoId { get; set; }
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Elev { get; set; }
        public List<string> Runways { get; set; }
    }
}