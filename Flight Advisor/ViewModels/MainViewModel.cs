// ViewModels/MainViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;
using FlightAdvisor.Models;
using FlightAdvisor.Services;
using System.Net.Http;

namespace FlightAdvisor.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        #region Fields

        private readonly WeatherService _weatherService;
        private readonly DecisionEngine _decisionEngine;
        private AircraftDatabase _aircraftDatabase;

        // Observable backing fields
        private string _selectedFlightType;
        private Aircraft _selectedAircraft;
        private string _departureIcao;
        private string _arrivalIcao;
        private string _alternateIcao;
        private int _takeoffWeight;
        private int _landingWeight;
        private int _soulsOnBoard;
        private double _fuelPlanned;
        private string _route;
        private string _selectedRunway;
        private FlightDecision _currentDecision;
        private WeatherSummary _weatherSummary;
        private bool _isLoading;
        private string _errorMessage;
        private bool _showAdvancedMode;
        private bool _showResults;
        private string _lastUpdateTime;
        private bool _showFlightDetails;
        private bool _isDarkMode;
        private string _headwindDisplay;
        private string _headwindColor;
        private int? _crosswindComponent;
        private string _crosswindColor;
        private string _autoSelectedRunwayInfo;
        private bool _showAutoSelectedInfo;
        private RunwayData _selectedRunwayData;
        private List<RunwayData> _cachedRunwayData = new List<RunwayData>();

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            _decisionEngine = new DecisionEngine(_weatherService);

            // Initialize theme based on current app theme
            if (Avalonia.Application.Current is App app)
            {
                _isDarkMode = app.IsDarkMode;
            }
            else
            {
                _isDarkMode = true; // Default to dark mode
            }

            // Initialize commands
            CheckWeatherCommand = ReactiveCommand.CreateFromTask(CheckWeatherAsync);
            SelectFlightTypeCommand = ReactiveCommand.Create<string>(SelectFlightType);
            ToggleAdvancedModeCommand = ReactiveCommand.Create(ToggleAdvancedMode);
            RefreshWeatherCommand = ReactiveCommand.CreateFromTask(RefreshWeatherAsync);
            ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);

            // Initialize collections
            FlightTypes = new ObservableCollection<string>
            {
                "Flight Lesson",
                "Gliding",
                "Recreational Flight",
                "Discovery Flight",
                "Just Looking at Weather"
            };

            DepartureRunways = new ObservableCollection<string>();
            ArrivalRunways = new ObservableCollection<string>();

            // Set default values
            ShowFlightDetails = false;
            SelectedFlightType = FlightTypes.First();

            // Load aircraft database
            LoadAircraftDatabase();
        }

        #endregion

        #region Properties

        public string SelectedFlightType
        {
            get => _selectedFlightType;
            set => this.RaiseAndSetIfChanged(ref _selectedFlightType, value);
        }

        public Aircraft SelectedAircraft
        {
            get => _selectedAircraft;
            set => this.RaiseAndSetIfChanged(ref _selectedAircraft, value);
        }

        public string DepartureIcao
        {
            get => _departureIcao;
            set
            {
                this.RaiseAndSetIfChanged(ref _departureIcao, value?.ToUpper());

                // Auto-load runways when ICAO code is complete (4 characters)
                if (!string.IsNullOrEmpty(value) && value.Length == 4)
                {
                    _ = LoadDepartureRunwaysAsync(value);
                }
            }
        }

        public string ArrivalIcao
        {
            get => _arrivalIcao;
            set
            {
                this.RaiseAndSetIfChanged(ref _arrivalIcao, value?.ToUpper());

                // Auto-load runways when ICAO code is complete (4 characters)
                if (!string.IsNullOrEmpty(value) && value.Length == 4)
                {
                    _ = LoadArrivalRunwaysAsync(value);
                }
            }
        }

        public string AlternateIcao
        {
            get => _alternateIcao;
            set => this.RaiseAndSetIfChanged(ref _alternateIcao, value?.ToUpper());
        }

        public int TakeoffWeight
        {
            get => _takeoffWeight;
            set => this.RaiseAndSetIfChanged(ref _takeoffWeight, value);
        }

        public int LandingWeight
        {
            get => _landingWeight;
            set => this.RaiseAndSetIfChanged(ref _landingWeight, value);
        }

        public int SoulsOnBoard
        {
            get => _soulsOnBoard;
            set => this.RaiseAndSetIfChanged(ref _soulsOnBoard, value);
        }

        public double FuelPlanned
        {
            get => _fuelPlanned;
            set => this.RaiseAndSetIfChanged(ref _fuelPlanned, value);
        }

        public string Route
        {
            get => _route;
            set => this.RaiseAndSetIfChanged(ref _route, value);
        }

        public string SelectedRunway
        {
            get => _selectedRunway;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedRunway, value);

                // Update selected runway data
                if (!string.IsNullOrEmpty(value) && value != "Auto-Selected")
                {
                    SelectedRunwayData = _cachedRunwayData.FirstOrDefault(r => r.DisplayName == value);
                }
                else
                {
                    SelectedRunwayData = null;
                }

                // Automatically recalculate wind components when runway changes
                RecalculateWindComponents();
            }
        }

        public string HeadwindDisplay
        {
            get => _headwindDisplay;
            set => this.RaiseAndSetIfChanged(ref _headwindDisplay, value);
        }

        public string HeadwindColor
        {
            get => _headwindColor;
            set => this.RaiseAndSetIfChanged(ref _headwindColor, value);
        }

        public int? CrosswindComponent
        {
            get => _crosswindComponent;
            set => this.RaiseAndSetIfChanged(ref _crosswindComponent, value);
        }

        public string CrosswindColor
        {
            get => _crosswindColor;
            set => this.RaiseAndSetIfChanged(ref _crosswindColor, value);
        }

        public string AutoSelectedRunwayInfo
        {
            get => _autoSelectedRunwayInfo;
            set => this.RaiseAndSetIfChanged(ref _autoSelectedRunwayInfo, value);
        }

        public bool ShowAutoSelectedInfo
        {
            get => _showAutoSelectedInfo;
            set => this.RaiseAndSetIfChanged(ref _showAutoSelectedInfo, value);
        }
        public RunwayData SelectedRunwayData
        {
            get => _selectedRunwayData;
            set => this.RaiseAndSetIfChanged(ref _selectedRunwayData, value);
        }
        public FlightDecision CurrentDecision
        {
            get => _currentDecision;
            set => this.RaiseAndSetIfChanged(ref _currentDecision, value);
        }

        public WeatherSummary WeatherSummary
        {
            get => _weatherSummary;
            set => this.RaiseAndSetIfChanged(ref _weatherSummary, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public bool ShowAdvancedMode
        {
            get => _showAdvancedMode;
            set => this.RaiseAndSetIfChanged(ref _showAdvancedMode, value);
        }

        public bool ShowResults
        {
            get => _showResults;
            set => this.RaiseAndSetIfChanged(ref _showResults, value);
        }

        public string LastUpdateTime
        {
            get => _lastUpdateTime;
            set => this.RaiseAndSetIfChanged(ref _lastUpdateTime, value);
        }

        public bool ShowFlightDetails
        {
            get => _showFlightDetails;
            set => this.RaiseAndSetIfChanged(ref _showFlightDetails, value);
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set => this.RaiseAndSetIfChanged(ref _isDarkMode, value);
        }

        // Collections
        public ObservableCollection<string> FlightTypes { get; }
        public ObservableCollection<Aircraft> AllAircraft { get; private set; }
        public ObservableCollection<string> DepartureRunways { get; }
        public ObservableCollection<string> ArrivalRunways { get; }

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> CheckWeatherCommand { get; }
        public ReactiveCommand<string, Unit> SelectFlightTypeCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleAdvancedModeCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshWeatherCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleThemeCommand { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load aircraft database from JSON file
        /// </summary>
        private void LoadAircraftDatabase()
        {
            try
            {
                var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Data", "aircraft.json");

                if (!File.Exists(jsonPath))
                {
                    ErrorMessage = "Aircraft database file not found.";
                    AllAircraft = new ObservableCollection<Aircraft>();
                    return;
                }

                var json = File.ReadAllText(jsonPath);
                _aircraftDatabase = JsonSerializer.Deserialize<AircraftDatabase>(json);

                if (_aircraftDatabase != null)
                {
                    AllAircraft = new ObservableCollection<Aircraft>(_aircraftDatabase.GetAllAircraft());

                    // Auto-select first aircraft if available
                    if (AllAircraft.Any())
                    {
                        SelectedAircraft = AllAircraft.First();
                    }
                }
                else
                {
                    AllAircraft = new ObservableCollection<Aircraft>();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load aircraft database: {ex.Message}";
                AllAircraft = new ObservableCollection<Aircraft>();
            }
        }

        /// <summary>
        /// Select flight type from UI
        /// </summary>
        private void SelectFlightType(string flightType)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                SelectedFlightType = flightType;
            });
        }

        /// <summary>
        /// Toggle advanced mode visibility
        /// </summary>
        private void ToggleAdvancedMode()
        {
            ShowAdvancedMode = !ShowAdvancedMode;
        }

        /// <summary>
        /// Toggle application theme
        /// </summary>
        private void ToggleTheme()
        {
            if (Avalonia.Application.Current is App app)
            {
                app.ToggleTheme();
                IsDarkMode = app.IsDarkMode;
            }
        }

        /// <summary>
        /// Load departure runways for specified airport
        /// </summary>
        private async Task LoadDepartureRunwaysAsync(string icao)
        {
            try
            {
                var runways = await FetchRunwayDataAsync(icao);

                DepartureRunways.Clear();

                // Always add "Auto-Selected" as first option
                DepartureRunways.Add("Auto-Selected");

                if (runways.Any())
                {
                    foreach (var runway in runways)
                    {
                        DepartureRunways.Add(runway.DisplayName);
                    }
                }

                // Default to Auto-Selected
                SelectedRunway = "Auto-Selected";
            }
            catch
            {
                // On error, provide auto-select option
                DepartureRunways.Clear();
                DepartureRunways.Add("Auto-Selected");
                SelectedRunway = "Auto-Selected";
            }
        }

        /// <summary>
        /// Load arrival runways for specified airport
        /// </summary>
        private async Task LoadArrivalRunwaysAsync(string icao)
        {
            try
            {
                var runways = await FetchRunwayDataAsync(icao);

                ArrivalRunways.Clear();

                // Always add "Auto-Selected" as first option
                ArrivalRunways.Add("Auto-Selected");

                if (runways.Any())
                {
                    foreach (var runway in runways)
                    {
                        ArrivalRunways.Add(runway.DisplayName);
                    }
                }
            }
            catch
            {
                // On error, provide auto-select option
                ArrivalRunways.Clear();
                ArrivalRunways.Add("Auto-Selected");
            }
        }

        /// <summary>
        /// Fetch runway data from NOAA Aviation Weather API
        /// </summary>
        private async Task<List<RunwayData>> FetchRunwayDataAsync(string icao)
        {
            string url = $"https://aviationweather.gov/api/data/airport?ids={icao}&format=json";
            using var client = new System.Net.Http.HttpClient();
            var runwayList = new List<RunwayData>();

            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "FlightAdvisor/1.0");
                var response = await client.GetStringAsync(url);

                System.Diagnostics.Debug.WriteLine($"Runway API Response: {response}");

                var airportArray = JsonDocument.Parse(response);

                if (airportArray.RootElement.GetArrayLength() > 0)
                {
                    var airportData = airportArray.RootElement[0];

                    if (airportData.TryGetProperty("runways", out var runwaysElement))
                    {
                        foreach (var runway in runwaysElement.EnumerateArray())
                        {
                            string id = null;
                            double heading = 0;
                            string dimension = null;
                            string surface = null;

                            // Try to get the runway ID
                            if (runway.TryGetProperty("id", out var idElement))
                            {
                                id = idElement.GetString();
                            }

                            // Try to get dimension
                            if (runway.TryGetProperty("dimension", out var dimElement))
                            {
                                dimension = dimElement.GetString();
                            }

                            // Try to get surface
                            if (runway.TryGetProperty("surface", out var surfElement))
                            {
                                surface = surfElement.GetString();
                            }

                            // Try to get the alignment/heading
                            if (runway.TryGetProperty("alignment", out var alignmentElement))
                            {
                                if (alignmentElement.ValueKind == JsonValueKind.Number)
                                {
                                    heading = alignmentElement.GetDouble();
                                }
                                else if (alignmentElement.ValueKind == JsonValueKind.String)
                                {
                                    var alignmentStr = alignmentElement.GetString();
                                    if (!string.IsNullOrWhiteSpace(alignmentStr) && alignmentStr != "-")
                                    {
                                        double.TryParse(alignmentStr, out heading);
                                    }
                                }
                            }

                            // If we have a valid runway ID, process it
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                // Check if runway ID contains a slash (e.g., "04L/22R", "11/29")
                                if (id.Contains("/"))
                                {
                                    var parts = id.Split('/');

                                    // First direction
                                    if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                                    {
                                        var rwy1 = parts[0].Trim();
                                        var heading1 = ParseRunwayHeading(rwy1);
                                        runwayList.Add(new RunwayData
                                        {
                                            Designator = rwy1,
                                            TrueHeading = heading1,
                                            DisplayName = $"{rwy1} ({heading1}°)",
                                            Dimension = dimension,
                                            Surface = surface
                                        });
                                    }

                                    // Second direction (reciprocal)
                                    if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                                    {
                                        var rwy2 = parts[1].Trim();
                                        var heading2 = ParseRunwayHeading(rwy2);
                                        runwayList.Add(new RunwayData
                                        {
                                            Designator = rwy2,
                                            TrueHeading = heading2,
                                            DisplayName = $"{rwy2} ({heading2}°)",
                                            Dimension = dimension,
                                            Surface = surface
                                        });
                                    }
                                }
                                else
                                {
                                    // Single runway or helipad
                                    var rwyHeading = heading > 0 ? heading : ParseRunwayHeading(id);
                                    runwayList.Add(new RunwayData
                                    {
                                        Designator = id,
                                        TrueHeading = rwyHeading,
                                        DisplayName = rwyHeading > 0 ? $"{id} ({rwyHeading}°)" : id,
                                        Dimension = dimension,
                                        Surface = surface
                                    });
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Found {runwayList.Count} runway options");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching runways: {ex.Message}");
                // Return empty list on error
            }

            return runwayList;
        }

        /// <summary>
        /// Select best runway based on wind conditions (most headwind)
        /// </summary>
        private string SelectBestRunway(List<RunwayData> runways, int windDirection, int windSpeed)
        {
            if (!runways.Any() || windSpeed == 0)
                return runways.FirstOrDefault()?.DisplayName ?? "Auto-Selected";

            // Calculate headwind component for each runway and select best
            var bestRunway = runways
                .Select(r => new
                {
                    Runway = r,
                    HeadwindComponent = CalculateHeadwindComponent(windDirection, windSpeed, (int)r.TrueHeading)
                })
                .OrderByDescending(x => x.HeadwindComponent)
                .FirstOrDefault();

            return bestRunway?.Runway.DisplayName ?? runways.First().DisplayName;
        }

        /// <summary>
        /// Calculate headwind component for runway selection
        /// </summary>
        private int CalculateHeadwindComponent(int windDirection, int windSpeed, int runwayHeading)
        {
            var angleDiff = Math.Abs(windDirection - runwayHeading);
            if (angleDiff > 180)
                angleDiff = 360 - angleDiff;

            var headwind = windSpeed * Math.Cos(angleDiff * Math.PI / 180);
            return (int)Math.Round(headwind);
        }

        /// <summary>
        /// Recalculate wind components when runway selection changes
        /// </summary>
        private void RecalculateWindComponents()
        {
            if (WeatherSummary == null ||
                !WeatherSummary.WindDirection.HasValue ||
                !WeatherSummary.WindSpeed.HasValue ||
                string.IsNullOrEmpty(SelectedRunway))
            {
                HeadwindDisplay = null;
                HeadwindColor = "#22c55e";
                CrosswindComponent = null;
                CrosswindColor = "#22c55e";
                return;
            }

            int runwayHeading;

            // If "Auto-Selected", find the best runway and use its heading
            if (SelectedRunway == "Auto-Selected")
            {
                var bestRunway = FindBestRunwayHeading(
                    WeatherSummary.WindDirection.Value,
                    WeatherSummary.WindSpeed.Value
                );
                runwayHeading = bestRunway;

                // Find the runway name from the heading
                var runwayName = DepartureRunways
                    .Where(r => r != "Auto-Selected")
                    .FirstOrDefault(r => ParseRunwayHeading(r) == bestRunway);

                AutoSelectedRunwayInfo = $"Using {runwayName ?? "best runway"} for calculations";
                ShowAutoSelectedInfo = true;
            }
            else
            {
                runwayHeading = ParseRunwayHeading(SelectedRunway);
                ShowAutoSelectedInfo = false;
            }

            var headwindValue = _weatherService.CalculateHeadwind(
                WeatherSummary.WindDirection.Value,
                WeatherSummary.WindSpeed.Value,
                runwayHeading);

            // Format headwind display with proper label and color
            if (headwindValue < 0)
            {
                // Tailwind - show as positive value with "Tailwind" label and orange color
                HeadwindDisplay = $"Tailwind {Math.Abs(headwindValue)}kts";
                HeadwindColor = "#f59e0b"; // Orange
            }
            else
            {
                // Headwind - show normally with green color
                HeadwindDisplay = $"{headwindValue}kts";
                HeadwindColor = "#22c55e"; // Green
            }

            var crosswindValue = _weatherService.CalculateCrosswind(
                WeatherSummary.WindDirection.Value,
                WeatherSummary.WindSpeed.Value,
                runwayHeading);

            CrosswindComponent = crosswindValue;

            // Set crosswind color based on thresholds
            if (crosswindValue < 10)
            {
                CrosswindColor = "#22c55e"; // Green - safe
            }
            else if (crosswindValue < 25)
            {
                CrosswindColor = "#f59e0b"; // Orange - caution
            }
            else
            {
                CrosswindColor = "#ef4444"; // Red - dangerous
            }
        }

        /// <summary>
        /// Find the best runway heading based on wind conditions (most headwind)
        /// </summary>
        private int FindBestRunwayHeading(int windDirection, int windSpeed)
        {
            // Get all available runways except "Auto-Selected"
            var availableRunways = DepartureRunways
                .Where(r => r != "Auto-Selected")
                .ToList();

            if (!availableRunways.Any())
                return 0;

            // Find runway with most headwind
            var bestRunway = availableRunways
                .Select(runway => new
                {
                    Runway = runway,
                    Heading = ParseRunwayHeading(runway),
                    HeadwindComponent = CalculateHeadwindComponent(windDirection, windSpeed, ParseRunwayHeading(runway))
                })
                .OrderByDescending(x => x.HeadwindComponent)
                .FirstOrDefault();

            return bestRunway?.Heading ?? 0;
        }

        /// <summary>
        /// Parse runway heading from runway designator (e.g., "09" -> 090°)
        /// </summary>
        private int ParseRunwayHeading(string runway)
        {
            // Extract numeric part from runway designator (e.g., "09L" -> "09")
            var numericPart = new string(runway.TakeWhile(char.IsDigit).ToArray());

            if (int.TryParse(numericPart, out var heading))
            {
                return heading * 10; // Convert to degrees (09 -> 090°)
            }

            return 0;
        }

        /// <summary>
        /// Main weather check operation
        /// </summary>
        private async Task CheckWeatherAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            ShowResults = false;

            try
            {
                // Validate input
                if (string.IsNullOrEmpty(DepartureIcao))
                {
                    ErrorMessage = "Please enter a departure airport code.";
                    return;
                }

                // Fetch METAR data
                var metar = await _weatherService.GetMetarAsync(DepartureIcao);

                if (metar == null)
                {
                    ErrorMessage = $"Unable to fetch weather data for {DepartureIcao}. Please verify the airport code and try again.";
                    return;
                }

                // Load runways and auto-select best one based on wind
                var runways = await FetchRunwayDataAsync(DepartureIcao);
                _cachedRunwayData = runways; // Cache the runway data
                DepartureRunways.Clear();
                DepartureRunways.Add("Auto-Selected");

                if (runways.Any())
                {
                    foreach (var rwy in runways)
                    {
                        DepartureRunways.Add(rwy.DisplayName);
                    }

                    // If wind data is available and user has "Auto-Selected", pick best runway
                    if (metar.WindDirection.HasValue && metar.WindSpeed.HasValue)
                    {
                        var bestRunway = SelectBestRunway(runways, metar.WindDirection.Value, metar.WindSpeed.Value);
                        SelectedRunway = bestRunway;
                    }
                    else
                    {
                        // No wind data, default to Auto-Selected
                        SelectedRunway = "Auto-Selected";
                    }
                }
                else
                {
                    SelectedRunway = "Auto-Selected";
                }

                // Fetch TAF data (optional)
                TafData taf = null;
                try
                {
                    taf = await _weatherService.GetTafAsync(DepartureIcao);
                }
                catch
                {
                    // TAF is optional - continue without it
                }

                // Build flight information object
                var flightInfo = new FlightInfo
                {
                    FlightType = SelectedFlightType,
                    SelectedAircraft = SelectedAircraft,
                    DepartureIcao = DepartureIcao,
                    ArrivalIcao = ArrivalIcao,
                    AlternateIcao = AlternateIcao,
                    TakeoffWeight = TakeoffWeight,
                    LandingWeight = LandingWeight,
                    SoulsOnBoard = SoulsOnBoard,
                    FuelPlanned = FuelPlanned,
                    Route = Route,
                    SelectedRunway = SelectedRunway
                };

                // Evaluate flight decision
                CurrentDecision = _decisionEngine.EvaluateFlight(metar, taf, flightInfo);
                WeatherSummary = CurrentDecision.WeatherSummary;

                // Add TAF to weather summary if available
                if (taf != null)
                {
                    WeatherSummary.RawTaf = taf.RawTaf;
                }

                // Update display
                LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
                ShowResults = true;

                // Recalculate wind components for selected runway
                RecalculateWindComponents();
            }
            catch (WeatherServiceException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Refresh weather data
        /// </summary>
        private async Task RefreshWeatherAsync()
        {
            if (!string.IsNullOrEmpty(DepartureIcao))
            {
                await CheckWeatherAsync();
            }
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Runway data structure with heading and display information
    /// </summary>
    public class RunwayData
    {
        public string Designator { get; set; }
        public double TrueHeading { get; set; }
        public string DisplayName { get; set; }
        public double Reciprocal => (TrueHeading + 180) % 360;

        // Additional runway details
        public string Dimension { get; set; }
        public string Surface { get; set; }
        public bool IsHelipad => Designator?.StartsWith("H") ?? false;
        public string RunwayType => IsHelipad ? "Helipad" : "Runway";

        // Parse length from dimension (e.g., "11000x150" -> 11000 ft)
        public int? LengthFeet
        {
            get
            {
                if (string.IsNullOrEmpty(Dimension)) return null;
                var parts = Dimension.Split('x');
                if (parts.Length > 0 && int.TryParse(parts[0], out var length))
                    return length;
                return null;
            }
        }

        // Parse width from dimension (e.g., "11000x150" -> 150 ft)
        public int? WidthFeet
        {
            get
            {
                if (string.IsNullOrEmpty(Dimension)) return null;
                var parts = Dimension.Split('x');
                if (parts.Length > 1 && int.TryParse(parts[1], out var width))
                    return width;
                return null;
            }
        }
    }
    #endregion
}