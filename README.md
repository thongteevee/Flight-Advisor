# ✈️ Flight Advisor

**Should I Go Flying Today?** - An intelligent weather decision tool for aviators

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Avalonia](https://img.shields.io/badge/Avalonia-11.0-9B4DCA?logo=avalonia)
![License](https://img.shields.io/badge/license-MIT-blue)

A modern, Garmin-inspired desktop application that helps pilots make informed go/no-go decisions based on real-time weather data from NOAA Aviation Weather services.

## 🎯 Target Users

- **Student Pilots** - New to aviation and learning weather decision-making
- **Flight Instructors** - Teaching weather evaluation and flight planning
- **Glider Pilots** - Specialized weather considerations for soaring
- **General Aviation Pilots** - Quick weather checks for VFR flights
- **Aviation Enthusiasts** - Anyone interested in airport weather conditions

## ✨ Features

### Current Release (MVP)
- ✅ **Real-time Weather Data** from NOAA Aviation Weather API
- ✅ **Intelligent Go/No-Go Decision Engine** with safety-first algorithms
- ✅ **20 Aircraft Database** (10 trainers + 10 gliders) with flight envelopes
- ✅ **Garmin-Style Professional UI** with smooth animations
- ✅ **Advanced Weather Analysis**:
  - Crosswind component calculation
  - Density altitude computation
  - Hazard detection (thunderstorms, icing, wind shear)
  - Cloud ceiling evaluation
- ✅ **Auto-Refresh** weather data every 5 minutes
- ✅ **Advanced Mode** with raw METAR/TAF data
- ✅ **Toast Notifications** for weather updates
- ✅ **Comprehensive Error Handling**

### Weather Safety Thresholds
Conservative limits for student/new pilots:
- Visibility: 550m minimum
- Crosswind: 15kts maximum
- Wind Gusts: 20kts maximum
- Temperature: -10°C to 35°C range
- Density Altitude: 5,500ft recommended limit
- No-go conditions: Snow, thunderstorms, freezing rain, icing

### Coming Soon
- 🔲 FAA API integration (NOTAM, runway data)
- 🔲 AI Chat Assistant (Claude integration)
- 🔲 Interactive flight route mapping
- 🔲 Multi-airport route weather analysis
- 🔲 Weather charts and graphs
- 🔲 Flight plan save/load functionality
- 🔲 Historical weather tracking

## 🚀 Quick Start

### Prerequisites
- Windows 10/11 (cross-platform support via Avalonia)
- .NET 8.0 SDK or later
- Visual Studio 2022 (recommended) or VS Code

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/flight-advisor.git
cd flight-advisor
```

2. **Install Avalonia Extension** (Visual Studio 2022)
   - Extensions → Manage Extensions
   - Search "Avalonia for Visual Studio 2022"
   - Install and restart

3. **Restore NuGet Packages**
```bash
dotnet restore
```

4. **Build and Run**
```bash
dotnet build
dotnet run
```

Or press `F5` in Visual Studio!

## 📦 Dependencies

```xml
<PackageReference Include="Avalonia" Version="11.0.*" />
<PackageReference Include="FluentAvalonia" Version="2.0.*" />
<PackageReference Include="ReactiveUI.Fody" Version="19.5.*" />
<PackageReference Include="Refit" Version="7.0.*" />
```

## 🎨 Screenshots

### Main Interface
Clean, professional Garmin-inspired design with real-time weather evaluation

### Decision Display
Clear go/no-go recommendations with detailed hazard analysis

### Advanced Mode
Raw METAR/TAF data for experienced pilots

## 🏗️ Architecture

```
FlightAdvisor/
├── Models/              # Data models (Weather, Aircraft, Decisions)
├── Services/            # Weather API, Decision Engine
├── ViewModels/          # ReactiveUI ViewModels (MVVM pattern)
├── Views/               # Avalonia XAML views
├── Converters/          # Value converters for data binding
└── Assets/Data/         # Aircraft database JSON
```

### Design Patterns
- **MVVM** (Model-View-ViewModel) with ReactiveUI
- **Repository Pattern** for data access
- **Service Layer** for business logic
- **Dependency Injection** ready architecture

## 🔧 Configuration

### Customize Safety Thresholds
Edit `Services/DecisionEngine.cs`:
```csharp
private const double MIN_VISIBILITY_METERS = 550;
private const int MAX_CROSSWIND_KTS = 15;
private const int MAX_GUST_KTS = 20;
// ... adjust as needed
```

### Change Auto-Refresh Interval
Edit `ViewModels/MainViewModel.cs`:
```csharp
_refreshTimer = new System.Timers.Timer(5 * 60 * 1000); // 5 minutes
```

## 📊 API Integration

### NOAA Aviation Weather API
- **Endpoint**: `https://aviationweather.gov/api/data/metar`
- **Authentication**: None (public API)
- **Rate Limit**: ~100 requests/minute
- **Documentation**: [aviationweather.gov](https://aviationweather.gov/data/api/)

### Future: FAA ADIP API
- Airport infrastructure data
- Runway information
- Requires client_id/client_secret (see `api.json`)

## 🧪 Testing

### Manual Test Scenarios
1. **Valid Airport**: Enter "KSFO" → Should show weather data
2. **Invalid Airport**: Enter "XXXX" → Should show friendly error
3. **Network Error**: Disconnect internet → Should handle gracefully
4. **Auto-Refresh**: Wait 5 minutes → Should show toast notification

### Aircraft Database Test
- 10 Trainers: C172, C152, C150, C182, DA40, SR20, PA28-140, PA28-161, PA28-181, DA20
- 10 Gliders: ASK21, ASK13, DG1000, GROB103, LS4, ASTIR, PW5, BLANIK, DISCUS, JANUS

## 🤝 Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines
- Follow C# naming conventions
- Use async/await for all API calls
- Add XML documentation comments
- Maintain MVVM pattern
- Test on multiple screen sizes

## 📝 License

This project is licensed under the MIT License - see LICENSE file for details.

## ⚠️ Disclaimer

**FOR ADVISORY USE ONLY**

This application provides weather information and recommendations for educational and planning purposes. Pilots must:
- Obtain official weather briefings before flight
- Make their own informed go/no-go decisions
- Follow all FAA regulations and procedures
- Verify aircraft limitations and personal minimums
- Never rely solely on this app for flight safety decisions

Always fly safely! 🛩️

## 🙏 Acknowledgments

- NOAA Aviation Weather Center for weather data
- FAA for airport infrastructure data
- Avalonia UI team for the excellent framework
- ReactiveUI for MVVM support
- Flight training community for safety guidelines

## 📫 Contact

- **Project Link**: [https://github.com/yourusername/flight-advisor](https://github.com/yourusername/flight-advisor)
- **Issues**: [https://github.com/yourusername/flight-advisor/issues](https://github.com/yourusername/flight-advisor/issues)
- **Discussions**: [https://github.com/yourusername/flight-advisor/discussions](https://github.com/yourusername/flight-advisor/discussions)

---

**Built with ❤️ for the aviation community**

*Clear skies and tailwinds!* ✈️
