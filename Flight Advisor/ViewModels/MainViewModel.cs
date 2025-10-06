// ViewModels/MainViewModel.cs - FIXED VERSION
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;
using FlightAdvisor.Models;
using FlightAdvisor.Services;
using Avalonia.Threading;

namespace FlightAdvisor.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly WeatherService _weatherService;
        private readonly DecisionEngine _decisionEngine;
        private AircraftDatabase _aircraftDatabase;
        private System.Timers.Timer _refreshTimer;

        // Observable Properties
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

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            _decisionEngine = new DecisionEngine(_weatherService);

            // Initialize IsDarkMode based on the actual app theme
            if (Avalonia.Application.Current is App app)
            {
                _isDarkMode = app.IsDarkMode;
            }
            else
            {
                _isDarkMode = true; // Default fallback
            }

            // Setup commands
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

            Runways = new ObservableCollection<string>();
            DepartureRunways = new ObservableCollection<string>();
            ArrivalRunways = new ObservableCollection<string>();

            // Start with flight details collapsed
            ShowFlightDetails = false;

            // Load aircraft database
            LoadAircraftDatabase();
        }

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
                if (!string.IsNullOrEmpty(value) && value.Length == 4)
                    _ = LoadDepartureRunwaysAsync(value);
            }
        }

        public string ArrivalIcao
        {
            get => _arrivalIcao;
            set
            {
                this.RaiseAndSetIfChanged(ref _arrivalIcao, value?.ToUpper());
                if (!string.IsNullOrEmpty(value) && value.Length == 4)
                    _ = LoadArrivalRunwaysAsync(value);
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
            set => this.RaiseAndSetIfChanged(ref _selectedRunway, value);
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

        public ObservableCollection<string> FlightTypes { get; }
        public ObservableCollection<Aircraft> AllAircraft { get; private set; }
        public ObservableCollection<string> Runways { get; }
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

        #region Methods

        private void LoadAircraftDatabase()
        {
            try
            {
                var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Data", "aircraft.json");
                var json = File.ReadAllText(jsonPath);
                _aircraftDatabase = JsonSerializer.Deserialize<AircraftDatabase>(json);

                AllAircraft = new ObservableCollection<Aircraft>(_aircraftDatabase.GetAllAircraft());
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load aircraft database: {ex.Message}";
            }
        }

        private void SelectFlightType(string flightType)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => SelectedFlightType = flightType);
        }

        private void ToggleAdvancedMode()
        {
            ShowAdvancedMode = !ShowAdvancedMode;
        }

        private async Task LoadRunwaysAsync(string icao)
        {
            try
            {
                var runways = await _weatherService.GetRunwaysAsync(icao);
                Runways.Clear();

                foreach (var runway in runways)
                {
                    Runways.Add(runway);
                }

                if (Runways.Any())
                    SelectedRunway = Runways.First();
            }
            catch (Exception ex)
            {
                Runways.Clear();
                Runways.Add("Auto-Selected");
                SelectedRunway = "Auto-Selected";
            }
        }

        private async Task LoadDepartureRunwaysAsync(string icao)
        {
            try
            {
                var runways = await FetchRunwayData(icao);
                DepartureRunways.Clear();

                foreach (var runway in runways)
                {
                    DepartureRunways.Add(runway);
                }

                if (DepartureRunways.Any())
                    SelectedRunway = DepartureRunways.First();
            }
            catch
            {
                DepartureRunways.Clear();
                DepartureRunways.Add("Auto-Selected");
            }
        }

        private async Task LoadArrivalRunwaysAsync(string icao)
        {
            try
            {
                var runways = await FetchRunwayData(icao);
                ArrivalRunways.Clear();

                foreach (var runway in runways)
                {
                    ArrivalRunways.Add(runway);
                }
            }
            catch
            {
                ArrivalRunways.Clear();
                ArrivalRunways.Add("Auto-Selected");
            }
        }

        private async Task<List<string>> FetchRunwayData(string icao)
        {
            string url = $"https://aviationweather.gov/api/data/airport?ids={icao}&format=json";
            using var client = new System.Net.Http.HttpClient();
            var runwayList = new List<string>();

            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "FlightAdvisor/1.0");
                var response = await client.GetStringAsync(url);
                var airportArray = System.Text.Json.JsonDocument.Parse(response);

                if (airportArray.RootElement.GetArrayLength() > 0)
                {
                    var airportData = airportArray.RootElement[0];

                    if (airportData.TryGetProperty("runways", out var runwaysElement))
                    {
                        foreach (var runway in runwaysElement.EnumerateArray())
                        {
                            if (runway.TryGetProperty("id", out var idElement))
                            {
                                var id = idElement.GetString();
                                if (!string.IsNullOrWhiteSpace(id))
                                {
                                    runwayList.Add(id);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return runwayList;
        }

        private async Task CheckWeatherAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            ShowResults = false;

            try
            {
                if (string.IsNullOrEmpty(DepartureIcao))
                {
                    ErrorMessage = "Please enter a departure airport code.";
                    return;
                }

                var metar = await _weatherService.GetMetarAsync(DepartureIcao);

                if (metar == null)
                {
                    ErrorMessage = $"Unable to fetch weather data for {DepartureIcao}. Please verify the airport code and try again.";
                    return;
                }

                TafData taf = null;
                try
                {
                    taf = await _weatherService.GetTafAsync(DepartureIcao);
                }
                catch
                {
                    // TAF is optional
                }

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

                CurrentDecision = _decisionEngine.EvaluateFlight(metar, taf, flightInfo);
                WeatherSummary = CurrentDecision.WeatherSummary;

                if (taf != null)
                    WeatherSummary.RawTaf = taf.RawTaf;

                LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
                ShowResults = true;

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

        private async Task RefreshWeatherAsync()
        {
            if (!string.IsNullOrEmpty(DepartureIcao))
            {
                await CheckWeatherAsync();
            }
        }

        private void ToggleTheme()
        {
            if (Avalonia.Application.Current is App app)
            {
                app.ToggleTheme();
                IsDarkMode = app.IsDarkMode;
            }
        }

        #endregion
    }
}