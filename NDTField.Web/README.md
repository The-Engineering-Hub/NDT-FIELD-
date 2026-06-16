# Field Labs — Building Safety Portal

A public-facing web portal that displays structural safety assessments of buildings across Kenya based on Non-Destructive Testing (NDT) data. Engineers upload reports from the **NDT Field** desktop application and the public can instantly search any assessed building to see its structural safety status.

---

## Overview

Building collapses remain a critical problem in Kenya. Field Labs addresses this by giving structural engineers a tool to publish NDT assessment results, and giving the public transparent access to building safety information — searchable by building name, location, or building number.

---

## Features

- 🔍 **Public building search** — search any assessed building by name, location or building number
- 🏗️ **Safety status display** — each building is rated Safe, Fair, or Dangerous with a score out of 100
- 📊 **Full test breakdown** — Schmidt Hammer and UPV (Direct and Indirect) test results per building
- 📤 **JSON report upload** — engineers upload reports exported from NDT Field desktop app
- 🔐 **Admin dashboard** — password-protected admin panel for managing and deleting reports
- 📱 **Mobile responsive** — fully usable on phones and tablets
- 📈 **Home page analytics** — live stats and doughnut chart showing safety breakdown across all buildings

---

## Safety Status Logic

| Status | Score | Meaning |
|---|---|---|
| ✅ Safe | 80 – 100 | All tested elements meet minimum structural requirements |
| ⚠️ Fair | 50 – 79 | Further investigation recommended for flagged elements |
| 🚨 Dangerous | 0 – 49 | Immediate structural investigation required — do not occupy |

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9.0 MVC |
| Language | C# |
| Database | SQLite via Entity Framework Core 9 |
| Frontend | Razor Views + Bootstrap 5 + Chart.js |
| Hosting | Railway |
| CI/CD | GitHub → Railway (auto-deploy on push to main) |

---

## How It Connects to NDT Field Desktop App

Field Labs is the web companion to the **NDT Field** desktop application — a Windows WPF app built in C# that engineers use in the field to record and calculate NDT test results.

### Workflow

```
Field Engineer
     │
     ▼
NDT Field Desktop App (WPF / Windows)
  • Records Schmidt Hammer readings
  • Records Direct and Indirect UPV readings
  • Calculates compressive strength, pulse velocity, safety score
     │
     ▼
Export as JSON
  • Engineer clicks "Export JSON" in the app
  • App saves report to local machine
  • App prompts: "Upload to Field Labs portal?"
     │
     ▼
Field Labs Web Portal (This Project)
  • Report received via REST API endpoint
  • Building stored in SQLite database
  • Public can immediately search and view the building
```

### API Endpoint

The desktop app sends the exported JSON to:

```
POST /api/buildings/upload
Content-Type: application/json
```

The API parses the JSON, extracts project info and safety summary, stores the full raw JSON for detailed view, and returns a confirmation with the building ID, status and score.

### JSON Structure

The JSON file exported by NDT Field and accepted by the API follows this structure:

```json
{
  "exportVersion": "1.0",
  "exportedAt": "2026-05-01T09:30:00",
  "exportedBy": "NDT Field v1.0.0",
  "project": {
    "name": "Westlands Plaza",
    "clientName": "Kamau & Associates",
    "location": "Westlands, Nairobi",
    "buildingNumber": "BLD-001",
    "operator": "J. Kamau",
    "dateOfTest": "2026-05-01"
  },
  "summary": {
    "totalTests": 12,
    "safetyStatus": "Safe",
    "safetyScore": 92,
    "excellentCount": 7,
    "goodCount": 5,
    "mediumCount": 0,
    "doubtfulCount": 0,
    "recommendation": "All tested elements meet minimum structural requirements."
  },
  "schmidtTests": [ ... ],
  "directUPVTests": [ ... ],
  "indirectUPVTests": [ ... ]
}
```

Full JSON schema documentation is available in [`/docs/json-schema.md`](docs/json-schema.md).

---

## Project Structure

```
NDTField.Web/
├── Controllers/
│   ├── HomeController.cs          # Home page with live stats
│   ├── BuildingsController.cs     # Public search, upload, details
│   ├── BuildingsApiController.cs  # REST API for desktop app
│   └── AdminController.cs         # Admin login and dashboard
├── Models/
│   ├── Building.cs                # Main data model
│   └── AppDbContext.cs            # Entity Framework DbContext
├── Views/
│   ├── Home/                      # Landing page
│   ├── Buildings/                 # Search, Upload, Details pages
│   ├── Admin/                     # Login and Dashboard
│   ├── Error/                     # 404 and 500 error pages
│   └── Shared/_Layout.cshtml      # Sidebar layout
├── wwwroot/
│   └── css/site.css               # All custom styles
├── Program.cs                     # App configuration and middleware
├── appsettings.json               # App settings (no secrets)
└── Dockerfile                     # Railway deployment config
```

---

## Running Locally

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 or VS Code

### Steps

```bash
# Clone the repository
git clone https://github.com/The-Engineering-Hub/NDT-FIELD-.git
cd field-labs-web

# Restore packages
dotnet restore

# Run the app
dotnet run
```

The app will be available at `https://localhost:7xxx`. The SQLite database (`ndtfield.db`) is created automatically on first run.

### Admin Login

Set your admin credentials in `appsettings.json` locally:

```json
"AdminCredentials": {
  "Username": "admin",
  "Password": "your-password-here"
}
```

In production these are set as environment variables on Railway — never commit real credentials to the repo.

---

## Deployment

The app is hosted on **Railway** and deploys automatically on every push to `main`.
Soon to be deployed on Engineering Hub domain.

### Environment Variables Required on Railway

| Variable | Description |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` |
| `ASPNETCORE_URLS` | Set to `http://+:8080` |
| `RAILWAY_VOLUME_MOUNT_PATH` | Path for persistent SQLite storage e.g. `/data` |
| `AdminCredentials__Username` | Admin login username |
| `AdminCredentials__Password` | Admin login password |

A Railway Volume must be attached to the service at `/data` for the SQLite database to persist across deploys.

---

## API Reference

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| `POST` | `/api/buildings/upload` | Upload JSON report from desktop app | None |
| `GET` | `/api/buildings/search?q=` | Search buildings by name or location | None |
| `GET` | `/api/buildings/{id}` | Get full building details | None |
| `GET` | `/api/buildings/{id}/status` | Get safety status only | None |

---

## Contributing

This project is maintained by the organization. To contribute:

1. Fork the repository
2. Create a feature branch — `git checkout -b feature/your-feature`
3. Commit your changes — `git commit -m "Add your feature"`
4. Push to the branch — `git push origin feature/your-feature`
5. Open a Pull Request

---

## Related Projects

- **NDT Field Desktop App** — WPF application for field data collection (private repository)

---

## License

This project is licensed under the MIT License.

---

## About

Field Labs is developed as part of a structural safety initiative to address the building collapse crisis in Kenya by making NDT assessment data publicly accessible and transparent.

Built with ❤️ by Gideon Kairu CEO Enzyme Studios.
