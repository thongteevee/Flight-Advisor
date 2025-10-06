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
        private int? _headwindComponent;
        private int? _crosswindComponent;

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
                // Automatically recalculate wind components when runway changes
                RecalculateWindComponents();
            }
        }

        public int? HeadwindComponent
        {
            get => _headwindComponent;
            set => this.RaiseAndSetIfChanged(ref _headwindComponent, value);
        }

        public int? CrosswindComponent
        {
            get => _crosswindComponent;
            set => this.RaiseAndSetIfChanged(ref _crosswindComponent, value);
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

                if (runways.Any())
                {
                    foreach (var runway in runways)
                    {
                        DepartureRunways.Add(runway.DisplayName);
                    }

                    // Auto-select first runway
                    if (DepartureRunways.Any())
                    {
                        SelectedRunway = DepartureRunways.First();
                    }
                }
                else
                {
                    DepartureRunways.Add("Auto-Selected");
                    SelectedRunway = "Auto-Selected";
                }
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

                if (runways.Any())
                {
                    foreach (var runway in runways)
                    {
                        ArrivalRunways.Add(runway.DisplayName);
                    }
                }
                else
                {
                    ArrivalRunways.Add("Auto-Selected");
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

                            // Try to get the runway ID
                            if (runway.TryGetProperty("id", out var idElement))
                            {
                                id = idElement.GetString();
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

                            // If we have a valid runway ID, add it
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                runwayList.Add(new RunwayData
                                {
                                    Designator = id,
                                    TrueHeading = heading,
                                    DisplayName = heading > 0 ? $"{id} ({heading:F0}°)" : id
                                });
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Found {runwayList.Count} runways");
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
                string.IsNullOrEmpty(SelectedRunway) ||
                SelectedRunway == "Auto-Selected")
            {
                HeadwindComponent = null;
                CrosswindComponent = null;
                return;
            }

            var runwayHeading = ParseRunwayHeading(SelectedRunway);

            HeadwindComponent = _weatherService.CalculateHeadwind(
                WeatherSummary.WindDirection.Value,
                WeatherSummary.WindSpeed.Value,
                runwayHeading);

            CrosswindComponent = _weatherService.CalculateCrosswind(
                WeatherSummary.WindDirection.Value,
                WeatherSummary.WindSpeed.Value,
                runwayHeading);
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
                if (runways.Any() && metar.WindDirection.HasValue && metar.WindSpeed.HasValue)
                {
                    DepartureRunways.Clear();
                    foreach (var rwy in runways)
                    {
                        DepartureRunways.Add(rwy.DisplayName);
                    }

                    // Auto-select runway with most headwind
                    SelectedRunway = SelectBestRunway(runways, metar.WindDirection.Value, metar.WindSpeed.Value);
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
    }

    #endregion
}