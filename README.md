# LASU Smart Parking System

An IoT-based smart parking system for Lagos State University (LASU), Ojo. Ultrasonic sensors detect whether a parking space is occupied, an ESP32 sends the status over Wi-Fi to an ASP.NET Core API, and drivers see live availability in a responsive web application, with driving directions to their destination faculty.

The pilot covers the **Faculty of Computing and Information Technology**, and the interface is designed to accommodate all faculties and schools on campus, allowing sensors to be added faculty by faculty.




---

## Features

- **Real-time availability:** Parking space status updates on screen within a second, without requiring a page refresh, using SignalR over WebSockets.
- **Campus-wide grid view:** Displays all 13 LASU faculties and schools, with 10 parking spaces each. The Faculty of Computing and Information Technology is the active pilot, while other faculties are marked as "Coming Soon" for future expansion.
- **Map view with navigation:** Select a destination from the parking grid, switch to the map, and get driving directions and estimated arrival time using the Google Maps Directions API.
- **Mobile-friendly interface:** Responsive layout designed for phones and desktop devices.
- **Connection status indicator:** Displays the live connection status and automatically reconnects when the connection drops.
- **Privacy-friendly detection:** Ultrasonic sensors measure distance to detect vehicle occupancy. The system does not use cameras or number plate recognition.

## How It Works

HC-SR04 Sensor
      |
      v
    ESP32
      |
      | HTTP POST (JSON)
      v
 ASP.NET Core API
      |
      v
  SQL Server
      |
      v
 SignalR Hub
      |
      v
 Web Application
 (Grid View + Map)

1. The ultrasonic sensor measures the distance to an object above the parking space.
2. The ESP32 compares the measurement against a predefined threshold to determine whether the space is occupied or available.
3. The ESP32 sends parking status updates to the ASP.NET Core API over Wi-Fi.
4. The API validates the incoming update, saves the new status to SQL Server, and broadcasts the update through SignalR.
5. Connected web clients receive the update and refresh the parking availability display instantly.

## Tech Stack

| Layer | Technology |
|---|---|
| Hardware | ESP32, HC-SR04 Ultrasonic Sensors, LED Indicators |
| Hardware Programming | Arduino C/C++ |
| Backend | ASP.NET Core Web API, C# |
| ORM | Entity Framework Core |
| Real-Time Communication | SignalR |
| Database | Microsoft SQL Server (LocalDB for development) |
| Frontend | HTML, CSS, JavaScript |
| Real-Time Client | SignalR JavaScript Client |
| Maps and Navigation | Google Maps JavaScript API, Directions API |

## Project Structure

SmartParkingSystem/
├── Backend/
│   └── SmartParkingAPI/
│       ├── Controllers/
│       │   └── ParkingController
│       ├── Models/
│       │   ├── ParkingSpace
│       │   ├── ParkingUpdateRequest
│       │   └── ParkingStatusResponse
│       ├── Data/
│       │   └── ParkingDbContext
│       ├── Hubs/
│       │   └── ParkingHub
│       ├── appsettings.json
│       └── Program.cs
│
├── Frontend/
│   ├── index.html
│   ├── config.example.js
│   ├── css/
│   │   └── styles.css
│   └── js/
│       ├── app.js
│       ├── map.js
│       └── signalr.min.js
│
├── Hardware/
│   └── ParkingSensor/
│       ├── ParkingSensor.ino
│       └── secrets.example.h
│
├── .gitignore
├── LICENSE
└── README.md

*The structure above represents the intended project organization. Some supporting files may vary depending on the current implementation.*

## Getting Started

### Prerequisites

- Visual Studio 2022 with the ASP.NET and web development workload.
- .NET 6 SDK.
- SQL Server LocalDB.
- VS Code with the Live Server extension, or another static file server.
- Google Maps API key with the Maps JavaScript API and Directions API enabled.
- Arduino IDE 2.x with the ESP32 board package installed.

### 1. Backend Setup

Navigate to the backend project directory:

```bash
cd Backend/SmartParkingAPI
```

Restore dependencies:

```bash
dotnet restore
```

Apply database migrations:

```bash
dotnet ef database update
```

Run the application:

```bash
dotnet run
```

Swagger should be available at:

```text
https://localhost:<port>/swagger
```

Ensure the CORS policy in `Program.cs` includes the origin from which the frontend is served (for example, `http://localhost:5500`) and allows credentials for SignalR connections.

If your development HTTPS certificate is not trusted, run:

```bash
dotnet dev-certs https --trust
```

### 2. Frontend Setup

1. Ensure `signalr.min.js` is available in `Frontend/js/`.
2. In `Frontend/js/app.js`, configure `API_BASE_URL` to point to your backend URL (for example, `https://localhost:7240`).
3. Copy `config.example.js` and rename the copy to `config.js`.
4. Add your own Google Maps API key to `config.js`.
5. Open `index.html` using the VS Code Live Server extension or another static file server.

**Note:** The actual `config.js` file is excluded from version control. You must provide your own Google Maps API key for map functionality.

### 3. Hardware Setup

1. Open `Hardware/ParkingSensor/ParkingSensor.ino` in the Arduino IDE.
2. Create a local `secrets.h` file using `secrets.example.h` as a template.
3. Add your Wi-Fi SSID and password to `secrets.h`.
4. Configure the backend API URL and parking space number in the firmware.
5. Select the appropriate ESP32 board and port.
6. Compile and upload the firmware to the ESP32.

**Note:** The actual `secrets.h` file is excluded from version control. Never commit your personal Wi-Fi credentials.

### Hardware Wiring

The following table shows the default ESP32 pin connections:

| Component | ESP32 Pin |
|---|---|
| HC-SR04 VCC | 5V (VIN) |
| HC-SR04 GND | GND |
| HC-SR04 TRIG | GPIO 5 |
| HC-SR04 ECHO | GPIO 18 |
| Green LED (+) | GPIO 2 (through a 220Ω resistor) |
| Red LED (+) | GPIO 4 (through a 220Ω resistor) |

**Important:** The HC-SR04 ECHO pin outputs 5V, while ESP32 GPIO pins operate at 3.3V. Use an appropriate voltage divider or level shifter before connecting ECHO to GPIO 18.

## API Reference

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/parking` | Retrieves all parking spaces. |
| GET | `/api/parking/{spaceNumber}` | Retrieves a specific parking space, e.g. `A1`. |
| POST | `/api/parking/update` | Updates a parking space's status (called by the ESP32). |
| GET | `/api/parking/stats` | Retrieves totals for active spaces, including available and occupied spaces. |
| WebSocket | `/parkinghub` | SignalR hub for real-time parking updates. |

### Parking Status Values

| Value | Status |
|---|---|
| 0 | Available |
| 1 | Occupied |
| 2 | Unavailable |

### Example Parking Update Request

```http
POST /api/parking/update
Content-Type: application/json
```

```json
{
  "spaceNumber": "A1",
  "status": 1,
  "sensorId": "ESP32_001"
}
```

Updates to parking spaces that are not active (`IsActive = false`) are rejected.

## Security Considerations

- Never commit your actual Google Maps API key. Keep it in `Frontend/config.js` and restrict it to authorized website referrers in Google Cloud Console.
- Keep Wi-Fi credentials in `Hardware/ParkingSensor/secrets.h`, not in the main Arduino sketch.
- Exclude local configuration and secret files from version control using `.gitignore`.
- Exclude build and development folders such as `bin/`, `obj/`, `.vs/`, and `appsettings.Development.json`.
- The parking update endpoint is currently open. Before real-world deployment, implement authentication or an API key/token validation mechanism for ESP32 devices.

## Roadmap

- [x] ASP.NET Core backend API.
- [x] SignalR real-time parking updates.
- [x] Responsive web application with live parking status.
- [x] Multi-faculty grid layout covering 13 faculties and schools.
- [x] Map view with driving directions and estimated arrival time.
- [ ] Faculty-aware backend schema, including a faculties table and coordinates for each faculty.
- [ ] ESP32 firmware improvements, including debounced detection and Wi-Fi reconnection.
- [ ] Extend sensor deployment to additional faculties.
- [ ] Historical parking data and an administrative dashboard.
- [ ] Deploy the frontend and host the backend.
- [ ] Parking reservation system and mobile application.


The initial pilot focuses on the Faculty of Computing and Information Technology at Lagos State University. The system is designed to support expansion to other faculties and schools as additional hardware is deployed.

## Author

**Roqeebah**
