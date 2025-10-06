// ViewModels/MainViewModel.cs
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

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            _decisionEngine = new DecisionEngine(_weatherService);

            // Setup commands
            CheckWeatherCommand = ReactiveCommand.CreateFromTask(CheckWeatherAsync);
            SelectFlightTypeCommand = ReactiveCommand.Create<string>(SelectFlightType);
            ToggleAdvancedModeCommand = ReactiveCommand.Create(ToggleAdvancedMode);
            RefreshWeatherCommand = ReactiveCommand.CreateFromTask(RefreshWeatherAsync);

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


            // Load aircraft database
            LoadAircraftDatabase();

            // Setup auto-refresh (5 minutes)
            SetupAutoRefresh();
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
                if (!string.IsNullOrEmpty(value))
                    _ = LoadRunwaysAsync(value);
            }
        }

        public string ArrivalIcao
        {
            get => _arrivalIcao;
            set => this.RaiseAndSetIfChanged(ref _arrivalIcao, value?.ToUpper());
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

        public ObservableCollection<string> FlightTypes { get; }
        public ObservableCollection<Aircraft> AllAircraft { get; private set; }
        public ObservableCollection<string> Runways { get; }

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> CheckWeatherCommand { get; }
        public ReactiveCommand<string, Unit> SelectFlightTypeCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleAdvancedModeCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshWeatherCommand { get; }

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
                // Silently fail for runway loading - not critical
                Runways.Clear();
                Runways.Add("Auto-Selected");
                SelectedRunway = "Auto-Selected";
            }
        }

        private async Task CheckWeatherAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            ShowResults = false;

            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DepartureIcao))
                {
                    ErrorMessage = "Please enter a departure airport code.";
                    return;
                }

                // Fetch weather data
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
                    // TAF is optional, continue without it
                }

                // Build flight info
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

                // Evaluate conditions
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

                // Show toast notification (will be handled in View)
                // For now, just update the timestamp
            }
        }

        private void SetupAutoRefresh()
        {
            _refreshTimer = new System.Timers.Timer(5 * 60 * 1000); // 5 minutes
            _refreshTimer.Elapsed += async (s, e) =>
            {
                if (ShowResults && !IsLoading)
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await RefreshWeatherAsync();
                    });
                }
            };
            _refreshTimer.Start();
        }

        #endregion
    }
}