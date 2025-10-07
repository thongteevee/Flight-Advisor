# ✈️ Flight Advisor

**Should I Go Flying Today?** - An intelligent weather decision tool for aviators

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Avalonia](https://img.shields.io/badge/Avalonia-11.3-9B4DCA?logo=avalonia)
![License](https://img.shields.io/badge/license-MIT-blue)

A modern, professional desktop application that helps pilots make informed go/no-go decisions based on real-time weather data from NOAA Aviation Weather services. Features a sleek, aviation-inspired UI with intelligent weather analysis and safety recommendations.

## 🎯 Target Users

- **Student Pilots** - Learn weather decision-making with guided safety recommendations
- **Flight Instructors** - Teaching tool for weather evaluation and flight planning
- **Glider Pilots** - Specialized weather considerations for soaring operations
- **General Aviation Pilots** - Quick weather checks and crosswind analysis for VFR flights
- **Aviation Enthusiasts** - Real-time airport weather conditions and METAR/TAF analysis

## ✨ Features

### Weather Analysis
- ✅ **Real-time METAR/TAF Data** from NOAA Aviation Weather API
- ✅ **Intelligent Go/No-Go Decision Engine** with safety-first algorithms
- ✅ **Multi-Airport Analysis** - Departure, arrival, and alternate airports
- ✅ **Runway-Specific Wind Analysis**:
  - Automatic runway selection based on wind direction
  - Crosswind and headwind/tailwind component calculations
  - Runway surface and dimension data
- ✅ **Advanced Weather Calculations**:
  - Density altitude computation
  - Temperature/dewpoint analysis
  - Visibility parsing (statute miles to meters)
  - Cloud ceiling evaluation
- ✅ **Hazard Detection**:
  - Thunderstorms, icing conditions, freezing rain
  - Low visibility, low ceilings, wind shear
  - Temperature extremes, high density altitude
  - Gusty wind conditions

### Aircraft Database
- ✅ **20 Aircraft Profiles** (10 trainers + 10 gliders)
- ✅ **Detailed Performance Envelopes**:
  - Maximum crosswind and wind speed limits
  - Stall speeds and operating altitudes
  - Minimum runway requirements
  - Category-specific limitations

### User Interface
- ✅ **Professional Aviation-Inspired Design** with smooth animations
- ✅ **Splash Screen** with animated loading sequence
- ✅ **Dark/Light Theme Toggle** (currently dark theme optimized)
- ✅ **Collapsible Flight Details** section for optional parameters
- ✅ **Airport Switcher Buttons** for quick navigation between departure/arrival/alternate
- ✅ **Advanced Mode** with raw METAR/TAF display
- ✅ **Color-Coded Decision Display**:
  - 🟢 **GO** - Conditions favorable (Green)
  - 🟡 **CAUTION** - Review hazards carefully (Amber)
  - 🔴 **NO-GO** - Flight not recommended (Red)

### Safety Features
- ✅ **Educational Quick Tips** for each hazard type
- ✅ **Actionable Recommendations** based on flight type
- ✅ **Comprehensive Error Handling** with user-friendly messages
- ✅ **Flexible JSON Parsing** to handle API variations
- ✅ **Unix Timestamp Support** for observation times

## 🛡️ Weather Safety Thresholds

Conservative limits designed for student and recreational pilots:

| Parameter | Threshold | Severity |
|-----------|-----------|----------|
| **Visibility** | 550m minimum | Critical |
| **Crosswind** | 15kts maximum | High |
| **Wind Gusts** | 20kts maximum | Critical |
| **Temperature Range** | -10°C to 35°C | Medium/High |
| **Density Altitude** | 5,500ft limit | High |
| **Cloud Ceiling** | 1,000ft minimum | High |

### Automatic No-Go Conditions
- ❌ Snow
- ❌ Thunderstorms
- ❌ Freezing Rain
- ❌ Severe Icing Conditions

## 🚀 Quick Start

### Prerequisites
- **Operating System**: Windows 10/11, Linux, or macOS
- **.NET 8.0 SDK** or later
- **IDE**: Visual Studio 2022 (recommended) or JetBrains Rider

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/flight-advisor.git
cd flight-advisor
```

2. **Install Avalonia Extension** (Visual Studio 2022)
   - Extensions → Manage Extensions
   - Search "Avalonia for Visual Studio 2022"
   - Install and restart Visual Studio

3. **Restore NuGet Packages**
```bash
dotnet restore
```

4. **Build the Solution**
```bash
dotnet build
```

5. **Run the Application**
```bash
cd "Flight Advisor"
dotnet run
```

Or simply press **F5** in Visual Studio!

## 📦 Dependencies

```xml
<!-- Core Framework -->
<PackageReference Include="Avalonia" Version="11.3.7" />
<PackageReference Include="Avalonia.Desktop" Version="11.3.7" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.3.7" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.3.7" />
<PackageReference Include="Avalonia.Fonts.Inter" Version="11.3.7" />

<!-- UI Components -->
<PackageReference Include="FluentAvaloniaUI" Version="2.4.0" />

<!-- MVVM & Reactive -->
<PackageReference Include="ReactiveUI" Version="22.0.1" />
<PackageReference Include="ReactiveUI.Fody" Version="19.5.41" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />

<!-- HTTP & API -->
<PackageReference Include="Refit" Version="8.0.0" />
<PackageReference Include="Refit.HttpClientFactory" Version="8.0.0" />

<!-- JSON Serialization -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="System.Text.Json" Version="9.0.9" />
```

## 🏗️ Project Structure

```
FlightAdvisor/
├── Assets/
│   └── Data/
│       └── aircraft.json          # Aircraft database
├── Converters/                     # XAML value converters
│   ├── ValueConverters.cs
│   ├── BoolToChevronConverter.cs
│   └── BoolToThemeIconConverter.cs
├── Models/                         # Data models
│   ├── AircraftModels.cs
│   ├── WeatherModels.cs
│   └── WeatherDecisionModels.cs
├── Services/                       # Business logic
│   ├── WeatherService.cs          # NOAA API integration
│   ├── DecisionEngine.cs          # Weather analysis logic
│   └── IWeatherApi.cs             # Refit API interface
├── ViewModels/                     # MVVM ViewModels
│   └── MainViewModel.cs
├── Views/                          # UI Views
│   ├── MainWindow.axaml
│   ├── MainWindow.axaml.cs
│   ├── SplashScreen.axaml
│   └── SplashScreen.axaml.cs
├── App.axaml                       # Application resources
├── App.axaml.cs                    # Application lifecycle
└── Program.cs                      # Entry point
```

## 🎨 Architecture

### Design Patterns
- **MVVM (Model-View-ViewModel)** with ReactiveUI for data binding
- **Service Layer Pattern** for weather API and decision logic
- **Repository Pattern** for aircraft data access
- **Converter Pattern** for UI data transformation
- **Dependency Injection Ready** architecture

### Key Components

#### WeatherService
Handles all NOAA API communication with robust error handling:
- Flexible JSON parsing for multiple response formats
- Custom converters for Unix timestamps, visibility, and nullable types
- Support for both array `[{...}]` and single object `{...}` responses

#### DecisionEngine
Analyzes weather data against safety thresholds:
- Multi-factor hazard evaluation
- Aircraft-specific performance considerations
- Educational recommendations and quick tips
- Runway-specific wind component calculations

#### MainViewModel
Central coordination of application state:
- Multi-airport weather management
- Runway selection and analysis
- UI state management (loading, errors, results)
- Flight type and aircraft selection

## 🔧 Configuration

### Customize Safety Thresholds
Edit `Services/DecisionEngine.cs`:
```csharp
private const double MIN_VISIBILITY_METERS = 550;
private const int MAX_CROSSWIND_KTS = 15;
private const int MAX_GUST_KTS = 20;
private const double MIN_TEMP_CELSIUS = -10;
private const double MAX_TEMP_CELSIUS = 35;
private const int MAX_ALTITUDE_FT = 5500;
```

### Add Aircraft to Database
Edit `Assets/Data/aircraft.json`:
```json
{
  "id": "YOUR_ID",
  "name": "Aircraft Name",
  "type": "Trainer",
  "category": "Single Engine Land",
  "maxCrosswind": 15,
  "maxWindSpeed": 20,
  "stallSpeedClean": 51,
  "maxOperatingAltitude": 14000,
  "minRunwayLength": 1335,
  "minVisibility": 3,
  "notes": "Your notes here"
}
```

## 📊 API Integration

### NOAA Aviation Weather API
- **Base URL**: `https://aviationweather.gov`
- **Endpoints Used**:
  - `/api/data/metar` - Current weather observations
  - `/api/data/taf` - Terminal aerodrome forecasts
  - `/api/data/airport` - Airport and runway information
- **Authentication**: None required (public API)
- **Rate Limit**: ~100 requests/minute
- **Documentation**: [aviationweather.gov/data/api/](https://aviationweather.gov/data/api/)

### Response Handling
The application handles multiple API response formats:
- Single object: `{ "icaoId": "KJFK", ... }`
- Array format: `[{ "icaoId": "KJFK", ... }]`
- Missing/null fields gracefully handled
- Unix timestamps and ISO 8601 dates supported

## 🧪 Testing

### Manual Test Scenarios

1. **Valid Airport Test**
   - Enter "KSFO" or "KJFK" → Should display current weather
   - Verify all fields populate correctly

2. **Multi-Airport Test**
   - Enter departure, arrival, and alternate airports
   - Use switcher buttons to navigate between airports
   - Verify independent weather data for each

3. **Runway Analysis Test**
   - Select different runways from dropdown
   - Verify crosswind/headwind calculations update
   - Check "Auto-Selected" matches wind direction

4. **Invalid Airport Test**
   - Enter "XXXX" or non-existent code
   - Should show friendly error message
   - No application crash

5. **Network Error Test**
   - Disconnect internet
   - Try to fetch weather
   - Should handle gracefully with error message

6. **Hazard Detection Test**
   - Find airport with poor weather (low visibility, high winds)
   - Verify NO-GO or CAUTION decision
   - Check hazard list displays correctly

### Aircraft Database
**Trainers** (10): C172, C152, C150, C182, DA40, SR20, PA28-140, PA28-161, PA28-181, DA20

**Gliders** (10): ASK21, ASK13, DG1000, GROB103, LS4, ASTIR, PW5, BLANIK, DISCUS, JANUS

## 🐛 Known Issues

- Light theme currently disabled (shows dark theme colors)
- Theme toggle button commented out in UI
- Auto-refresh timer not yet implemented
- TAF data displayed but not used in decision logic

## 🚧 Roadmap

### Short Term (v1.1)
- [ ] Fix light theme implementation
- [ ] Add auto-refresh timer (5-minute intervals)
- [ ] Implement TAF-based forecast hazards
- [ ] Add notification system for weather updates
- [ ] Save/load last used airports

### Medium Term (v1.5)
- [ ] Historical weather tracking
- [ ] Weather trends and graphs
- [ ] Flight plan save/load functionality
- [ ] Export decision reports (PDF/text)
- [ ] User-customizable safety thresholds

### Long Term (v2.0)
- [ ] AI chat assistant integration
- [ ] Interactive route mapping
- [ ] Multi-leg route weather analysis
- [ ] NOTAM integration
- [ ] Mobile companion app

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/AmazingFeature
   ```
3. **Commit your changes**
   ```bash
   git commit -m 'Add some AmazingFeature'
   ```
4. **Push to the branch**
   ```bash
   git push origin feature/AmazingFeature
   ```
5. **Open a Pull Request**

### Development Guidelines
- Follow C# naming conventions (PascalCase for public, camelCase for private)
- Add XML documentation comments for public APIs
- Use async/await for all I/O operations
- Maintain MVVM pattern separation
- Test on multiple screen resolutions
- Handle all exceptions gracefully

### Code Style
```csharp
// Good
public async Task<MetarData> GetMetarAsync(string icao)
{
    try
    {
        var response = await _httpClient.GetAsync($"/api/data/metar?ids={icao}");
        return await ProcessResponse(response);
    }
    catch (Exception ex)
    {
        throw new WeatherServiceException($"Failed to fetch METAR: {ex.Message}", ex);
    }
}
```

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ⚠️ Disclaimer

**FOR ADVISORY USE ONLY - NOT FOR FLIGHT PLANNING**

This application provides weather information and recommendations for **educational and planning purposes only**. Pilots must:

- ✈️ Obtain official weather briefings before every flight
- ✈️ Make their own informed go/no-go decisions
- ✈️ Follow all FAA/CAA regulations and procedures
- ✈️ Verify aircraft limitations and personal minimums
- ✈️ Never rely solely on this app for flight safety decisions
- ✈️ Consult certified flight instructors when in doubt

**The developer assumes no liability for decisions made using this software.**

Always fly safely! 🛩️

## 🙏 Acknowledgments

- **NOAA Aviation Weather Center** - For providing free, real-time weather data
- **Avalonia UI Team** - For the excellent cross-platform UI framework
- **ReactiveUI Team** - For powerful MVVM support
- **FluentAvalonia** - For beautiful Fluent Design components
- **Aviation Community** - For safety guidelines and best practices
- **All Contributors** - Thank you for making this project better!

## 📫 Contact & Support

- **Project Repository**: [github.com/yourusername/flight-advisor](https://github.com/yourusername/flight-advisor)
- **Report Issues**: [GitHub Issues](https://github.com/yourusername/flight-advisor/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/flight-advisor/discussions)
- **Email**: your.email@example.com

### Support the Project
If you find this project useful:
- ⭐ Star the repository
- 🐛 Report bugs and issues
- 💡 Suggest new features
- 🔧 Submit pull requests
- 📢 Share with other pilots

---

**Built with ❤️ for the aviation community**

*Clear skies and tailwinds!* ✈️☁️

---

### Version History

**v1.0.0** (Current)
- Initial release with core weather analysis features
- 20 aircraft database (trainers + gliders)
- Multi-airport support with runway analysis
- Intelligent go/no-go decision engine
- Professional aviation-inspired UI

---

**Last Updated**: October 2025
