using System;
using System.Collections.Generic;
using System.Linq;
using FlightAdvisor.Models;

namespace FlightAdvisor.Services
{
    public class DecisionEngine
    {
        private readonly WeatherService _weatherService;

        private const double MIN_VISIBILITY_METERS = 550;
        private const int MAX_CROSSWIND_KTS = 15;
        private const int MAX_GUST_KTS = 20;
        private const double MIN_TEMP_CELSIUS = -10;
        private const double MAX_TEMP_CELSIUS = 35;
        private const int MAX_ALTITUDE_FT = 5500;

        public DecisionEngine(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public FlightDecision EvaluateFlight(MetarData metar, TafData taf, FlightInfo flightInfo)
        {
            var decision = new FlightDecision
            {
                Decision = DecisionType.Go,
                WeatherSummary = BuildWeatherSummary(metar, flightInfo)
            };

            var hazards = new List<WeatherHazard>();

            // 1. Check Visibility
            var visibilityMeters = _weatherService.ParseVisibility(metar.Visibility);
            if (visibilityMeters < MIN_VISIBILITY_METERS)
            {
                hazards.Add(new WeatherHazard
                {
                    Type = "Visibility",
                    Description = $"Visibility is {(int)visibilityMeters}m, below safe minimum.",
                    Severity = HazardSeverity.Critical,
                    Value = $"{(int)visibilityMeters}m",
                    Threshold = $"{MIN_VISIBILITY_METERS}m minimum",
                    QuickTip = "Low visibility can make it difficult to maintain visual reference with the ground and see other traffic. VFR flight requires adequate visibility for safe navigation."
                });
                decision.Decision = DecisionType.NoGo;
            }

            // 2. Check Crosswind
            if (!string.IsNullOrEmpty(flightInfo.SelectedRunway) && metar.WindDirection.HasValue && metar.WindSpeed.HasValue)
            {
                var runwayHeading = ParseRunwayHeading(flightInfo.SelectedRunway);
                var crosswind = _weatherService.CalculateCrosswind(
                    metar.WindDirection.Value,
                    metar.WindSpeed.Value,
                    runwayHeading
                );

                decision.WeatherSummary.CrosswindComponent = crosswind;

                if (crosswind > MAX_CROSSWIND_KTS)
                {
                    hazards.Add(new WeatherHazard
                    {
                        Type = "Crosswind",
                        Description = $"Crosswind component is {crosswind}kts, exceeding recommended limit.",
                        Severity = HazardSeverity.High,
                        Value = $"{crosswind}kts",
                        Threshold = $"{MAX_CROSSWIND_KTS}kts maximum",
                        QuickTip = "Excessive crosswinds make landing challenging. Student pilots should avoid crosswinds above their skill level. Consider a different runway or postponing the flight."
                    });

                    if (decision.Decision == DecisionType.Go)
                        decision.Decision = DecisionType.Caution;
                }
            }

            // 3. Check Wind Gusts
            if (metar.WindGust.HasValue && metar.WindGust.Value > MAX_GUST_KTS)
            {
                hazards.Add(new WeatherHazard
                {
                    Type = "Wind Gust",
                    Description = $"Wind gusts of {metar.WindGust}kts exceed safe limits.",
                    Severity = HazardSeverity.High,
                    Value = $"{metar.WindGust}kts",
                    Threshold = $"{MAX_GUST_KTS}kts maximum",
                    QuickTip = "Gusty winds can cause sudden changes in aircraft performance and control. They're especially challenging during takeoff and landing."
                });
                decision.Decision = DecisionType.NoGo;
            }

            // 4. Check Temperature Extremes
            if (metar.Temperature.HasValue)
            {
                if (metar.Temperature.Value < MIN_TEMP_CELSIUS)
                {
                    hazards.Add(new WeatherHazard
                    {
                        Type = "Low Temperature",
                        Description = $"Temperature is {metar.Temperature}\u00B0C, below safe operating limit.",
                        Severity = HazardSeverity.High,
                        Value = $"{metar.Temperature}\u00B0C",
                        Threshold = $"{MIN_TEMP_CELSIUS}\u00B0C minimum",
                        QuickTip = "Cold temperatures can affect aircraft performance, fuel systems, and increase icing risk. Pre-heat the engine and check for ice accumulation."
                    });

                    if (decision.Decision == DecisionType.Go)
                        decision.Decision = DecisionType.Caution;
                }

                if (metar.Temperature.Value > MAX_TEMP_CELSIUS)
                {
                    hazards.Add(new WeatherHazard
                    {
                        Type = "High Temperature",
                        Description = $"Temperature is {metar.Temperature}\u00B0C, approaching maximum safe limit.",
                        Severity = HazardSeverity.Medium,
                        Value = $"{metar.Temperature}\u00B0C",
                        Threshold = $"{MAX_TEMP_CELSIUS}\u00B0C maximum",
                        QuickTip = "High temperatures reduce air density, degrading aircraft performance. Expect longer takeoff rolls and reduced climb rates. Calculate density altitude before flight."
                    });

                    if (decision.Decision == DecisionType.Go)
                        decision.Decision = DecisionType.Caution;
                }
            }

            // 5. Check Density Altitude
            if (metar.Temperature.HasValue && metar.Altimeter.HasValue && metar.Elevation.HasValue)
            {
                var fieldElevation = metar.Elevation.Value; // Remove Math.Abs()

                var densityAltitude = _weatherService.CalculateDensityAltitude(
                    fieldElevation,
                    metar.Temperature.Value,
                    metar.Altimeter.Value
                );

                decision.WeatherSummary.DensityAltitude = densityAltitude;

                if (densityAltitude > MAX_ALTITUDE_FT)
                {
                    hazards.Add(new WeatherHazard
                    {
                        Type = "Density Altitude",
                        Description = $"Density altitude is {densityAltitude}ft, significantly degrading performance.",
                        Severity = HazardSeverity.High,
                        Value = $"{densityAltitude}ft",
                        Threshold = $"{MAX_ALTITUDE_FT}ft recommended limit",
                        QuickTip = "High density altitude reduces engine power, propeller efficiency, and wing lift. Performance will be noticeably degraded\u2014plan for longer takeoff and landing distances."
                    });

                    if (decision.Decision == DecisionType.Go)
                        decision.Decision = DecisionType.Caution;
                }
            }

            // 6. Check for Precipitation/Weather Conditions
            var weatherHazards = _weatherService.ParseWeatherHazards(metar.WeatherString);

            if (weatherHazards.Contains("Snow"))
            {
                hazards.Add(new WeatherHazard
                {
                    Type = "Snow",
                    Description = "Snow conditions present. VFR flight not recommended.",
                    Severity = HazardSeverity.Critical,
                    Value = "Active",
                    Threshold = "None acceptable",
                    QuickTip = "Snow reduces visibility dramatically and can cause icing. Most GA aircraft lack adequate anti-icing equipment. Do not fly in snow unless properly equipped and rated."
                });
                decision.Decision = DecisionType.NoGo;
                decision.WeatherSummary.HasPrecipitation = true;
            }

            if (weatherHazards.Contains("Thunderstorms"))
            {
                hazards.Add(new WeatherHazard
                {
                    Type = "Thunderstorms",
                    Description = "Thunderstorm activity reported. Flight is unsafe.",
                    Severity = HazardSeverity.Critical,
                    Value = "Active",
                    Threshold = "None acceptable",
                    QuickTip = "Never fly near thunderstorms. They contain extreme turbulence, hail, lightning, and severe icing. Maintain at least 20nm distance from any thunderstorm cell."
                });
                decision.Decision = DecisionType.NoGo;
                decision.WeatherSummary.HasThunderstorms = true;
            }

            if (weatherHazards.Contains("Freezing Rain"))
            {
                hazards.Add(new WeatherHazard
                {
                    Type = "Icing",
                    Description = "Freezing rain creates severe icing conditions.",
                    Severity = HazardSeverity.Critical,
                    Value = "Active",
                    Threshold = "None acceptable",
                    QuickTip = "Freezing rain is one of the most dangerous weather conditions. Ice accumulates rapidly on aircraft surfaces. Do not fly without certified anti-ice/de-ice equipment."
                });
                decision.Decision = DecisionType.NoGo;
                decision.WeatherSummary.HasIcing = true;
            }

            // 7. Check TAF for Wind Shear
            if (taf?.Forecasts != null)
            {
                foreach (var forecast in taf.Forecasts)
                {
                    if (forecast.WindShearHeight.HasValue && forecast.WindShearHeight.Value < 2000)
                    {
                        hazards.Add(new WeatherHazard
                        {
                            Type = "Wind Shear",
                            Description = $"Wind shear forecast at {forecast.WindShearHeight}ft.",
                            Severity = HazardSeverity.High,
                            Value = $"{forecast.WindShearHeight}ft",
                            Threshold = "Below 2000ft is hazardous",
                            QuickTip = "Wind shear causes sudden changes in wind speed/direction. It's particularly dangerous during takeoff and landing. Expect significant airspeed fluctuations."
                        });

                        if (decision.Decision == DecisionType.Go)
                            decision.Decision = DecisionType.Caution;
                    }
                }
            }

            // 8. Check Cloud Ceiling
            if (metar.Clouds != null && metar.Clouds.Any())
            {
                var lowestCeiling = metar.Clouds
                    .Where(c => c.Cover == "OVC" || c.Cover == "BKN")
                    .OrderBy(c => c.Base)
                    .FirstOrDefault();

                if (lowestCeiling?.Base < 1000)
                {
                    hazards.Add(new WeatherHazard
                    {
                        Type = "Low Ceiling",
                        Description = $"Cloud ceiling at {lowestCeiling.Base}ft is below VFR minimums.",
                        Severity = HazardSeverity.High,
                        Value = $"{lowestCeiling.Base}ft {lowestCeiling.Cover}",
                        Threshold = "1000ft minimum for training",
                        QuickTip = "Low ceilings limit maneuvering room and can lead to inadvertent IMC (instrument meteorological conditions). VFR requires maintaining cloud clearances."
                    });

                    if (decision.Decision == DecisionType.Go)
                        decision.Decision = DecisionType.Caution;
                }
            }

            decision.Hazards = hazards;
            decision.Summary = GenerateSummary(decision.Decision, hazards);
            decision.Recommendations = GenerateRecommendations(decision.Decision, hazards, flightInfo);
            decision.Cautions = GenerateCautions(hazards);

            return decision;
        }

        private WeatherSummary BuildWeatherSummary(MetarData metar, FlightInfo flightInfo)
        {
            var fieldElevation = metar.Elevation ?? 0;

            var summary = new WeatherSummary
            {
                AirportIcao = metar.IcaoId,
                AirportName = metar.AirportName ?? metar.IcaoId,
                ObservationTime = metar.ObservationTime,
                Temperature = metar.Temperature,
                DewPoint = metar.DewPoint,
                WindDirection = metar.WindDirection,
                WindSpeed = metar.WindSpeed,
                WindGust = metar.WindGust,
                Visibility = metar.Visibility,
                Altimeter = metar.Altimeter,
                Elevation = fieldElevation,
                WeatherConditions = metar.WeatherString,
                RawMetar = metar.RawObservation
            };

            if (metar.Clouds != null)
            {
                summary.CloudLayers = metar.Clouds
                    .Select(c => $"{c.Cover} at {c.Base}ft")
                    .ToList();
            }

            return summary;
        }

        private string GenerateSummary(DecisionType decision, List<WeatherHazard> hazards)
        {
            return decision switch
            {
                DecisionType.Go => "Conditions are favorable for VFR flight. Standard precautions apply.",
                DecisionType.Caution => $"\u26A0\uFE0F Flight possible with caution. {hazards.Count} concern(s) identified. Review all hazards before departure.",
                DecisionType.NoGo => $"\u274C Flight NOT recommended. {hazards.Where(h => h.Severity >= HazardSeverity.High).Count()} critical hazard(s) present.",
                _ => "Weather information displayed for reference only."
            };
        }

        private List<string> GenerateRecommendations(DecisionType decision, List<WeatherHazard> hazards, FlightInfo flightInfo)
        {
            var recommendations = new List<string>();

            if (decision == DecisionType.NoGo)
            {
                recommendations.Add("Consider postponing the flight until conditions improve.");
                recommendations.Add("Monitor weather updates and TAF forecasts for improvement.");

                if (hazards.Any(h => h.Type.Contains("Wind")))
                    recommendations.Add("If winds are the primary concern, consider flying later in the day when winds typically calm.");
            }
            else if (decision == DecisionType.Caution)
            {
                recommendations.Add("Ensure you have recent flight experience in similar conditions.");
                recommendations.Add("Consider flying with a CFI if you're not current on these conditions.");
                recommendations.Add("Have alternate plans ready in case conditions deteriorate.");

                if (flightInfo.FlightType == "FlightLesson" || flightInfo.FlightType == "DiscoveryFlight")
                    recommendations.Add("For training flights, consider postponing to provide better learning conditions.");
            }
            else
            {
                recommendations.Add("Complete a thorough preflight inspection.");
                recommendations.Add("File a flight plan or use flight following for added safety.");
                recommendations.Add("Monitor weather updates during flight.");
            }

            return recommendations;
        }

        private List<string> GenerateCautions(List<WeatherHazard> hazards)
        {
            return hazards
                .Where(h => h.Severity >= HazardSeverity.Medium)
                .Select(h => h.Description)
                .ToList();
        }

        private int ParseRunwayHeading(string runway)
        {
            var numericPart = new string(runway.TakeWhile(char.IsDigit).ToArray());

            if (int.TryParse(numericPart, out var heading))
            {
                return heading * 10;
            }

            return 0;
        }
    }
}