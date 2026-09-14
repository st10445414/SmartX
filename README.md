# Smart-X IoT Mesh Ecosystem

## Overview

Smart-X is a .NET 10-based Internet of Things (IoT) telemetry ingestion and monitoring application developed as part of the PROG7312 Portfolio of Evidence.

The application simulates a distributed IoT environment in which multiple sensor devices send different forms of telemetry to a central gateway.

The system currently implements the first architectural pillar of the Smart-X ecosystem:

**Sensor Data Ingestion and Telemetry**

The remaining architectural pillars are displayed in the application but are disabled for future development:

* Real-Time Command Stream and History
* Network Topology and Mesh Routing

Smart-X demonstrates the use of advanced C# and .NET concepts including generics, operator overloading, jagged arrays, recursion, generic collections, asynchronous API communication, validation, file uploads, and dynamic sensor health monitoring.

---

# Solution Architecture

The solution contains two main projects:

```text
SmartX
│
├── SmartX.Api
│   ├── Controllers
│   ├── Models
│   ├── Repositories
│   ├── Services
│   ├── Uploads
│   └── Program.cs
│
├── SmartX.Client
│   ├── Controllers
│   ├── Models
│   ├── Services
│   ├── Views
│   ├── wwwroot
│   └── Program.cs
│
├── SmartX.sln
└── README.md
```

## SmartX.Api

The API project acts as the backend telemetry gateway.

Its responsibilities include:

* Registering Smart-X sensors
* Receiving telemetry data
* Validating telemetry
* Detecting anomalous readings
* Monitoring sensor connectivity
* Managing telemetry history
* Handling different telemetry data types
* Managing sensor file attachments
* Providing data to the client application

## SmartX.Client

The client project provides the user-facing Smart-X management interface.

Its responsibilities include:

* Sensor registration
* Sensor listing
* Sensor health dashboard
* Warning and anomaly feedback
* Sensor file uploads
* Communication with the Smart-X API
* Displaying connected, warning, and disconnected devices

---

# Technologies Used

The Smart-X application uses the following technologies:

* .NET 10
* C#
* ASP.NET Core Web API
* ASP.NET Core MVC
* Razor Views
* Bootstrap
* RESTful API communication
* JSON
* Swagger / OpenAPI
* HTML
* CSS
* JavaScript

---

# Functional Features

## 1. Smart-X Startup Interface

The landing page displays the three architectural pillars of the Smart-X ecosystem.

### Available Module

* Sensor Data Ingestion and Telemetry

### Future Modules

* Real-Time Command Stream and History
* Network Topology and Mesh Routing

The future modules are intentionally disabled because they form part of later Smart-X development phases.

---

# 2. Sensor Registration

Users can register a Smart-X sensor through the client application.

Each sensor contains information such as:

* Device name
* MAC address
* Deployment location
* Zone
* Sensor category
* Registration date
* Last communication time
* Sensor status

Supported sensor categories include:

* Environmental
* Temperature
* Humidity
* Power Consumption
* Pressure
* Motion
* Actuator
* Other

Validation is performed before sensor information is submitted to the backend API.

The MAC address is validated using the expected hexadecimal MAC address structure.

Example:

```text
AA:BB:CC:DD:EE:01
```

Duplicate MAC addresses are rejected by the API.

---

# 3. API Integration

The Smart-X client communicates with the backend using asynchronous HTTP communication.

The general data flow is:

```text
User
  ↓
Smart-X Client
  ↓
MVC Controller
  ↓
HttpClient
  ↓
Smart-X Web API
  ↓
Validation / Service Layer
  ↓
Repository / Telemetry Store
```

Asynchronous communication is used to help prevent the client interface from becoming blocked while API requests are processed.

---

# 4. Generic Telemetry Processing

Smart-X uses a reusable generic class called:

```csharp
TelemetryPacket<T>
```

This allows the application to process different telemetry types without storing all incoming values as `object`.

Examples include:

```csharp
TelemetryPacket<double>
```

for decimal readings such as temperature and humidity,

```csharp
TelemetryPacket<int>
```

for integer readings such as power consumption,

and

```csharp
TelemetryPacket<bool>
```

for actuator states such as whether a valve is open or closed.

This approach reduces unnecessary type conversion and avoids unnecessary boxing and unboxing.

Example telemetry values include:

```text
Temperature = 24.8
Power = 850
ValveState = true
```

---

# 5. Operator Overloading

Smart-X uses operator overloading to simplify operations on compatible numerical sensor readings.

The `NumericSensorReading<T>` structure supports operations such as:

```text
+
-
>
<
```

For example:

```text
Meter 1 = 450 W
Meter 2 = 350 W

Combined Power = 800 W
```

The application can therefore perform:

```csharp
var combined = meterOne + meterTwo;
```

rather than manually extracting and adding each sensor value.

The overloaded comparison operators can also be used to compare readings against thresholds.

Example:

```csharp
combined > 750
```

---

# 6. Telemetry Validation and Anomaly Detection

Incoming telemetry is validated before being stored.

Smart-X can identify telemetry values that fall outside expected operating ranges.

Examples include:

```text
Temperature below 0°C or above 40°C
Humidity below 20% or above 80%
Pressure below 950 or above 1050
Power below 0 W or above 5000 W
```

When an anomalous reading is detected, the associated sensor is placed into a warning state.

The anomaly is then displayed prominently on the Smart-X dashboard.

---

# 7. Dynamic Sensor Health Dashboard

The dashboard implements the engagement strategy identified during the Smart-X research phase:

**Interactive Sensor Health and Anomaly Monitoring**

The dashboard provides summary information including:

* Total sensors
* Connected sensors
* Sensors requiring attention
* Disconnected sensors

Sensor health is calculated using the time since the sensor last transmitted telemetry.

The current health rules are:

```text
0–5 minutes:
Connected

More than 5 minutes but no more than 15 minutes:
Warning

More than 15 minutes:
Disconnected
```

A sensor can also enter a warning state if an anomalous telemetry value is detected.

Disconnected and warning sensors are visually prioritised so that problems can be identified quickly.

The dashboard refreshes periodically to reflect changing sensor conditions.

---

# 8. Jagged Arrays

Smart-X uses jagged arrays to represent batches of historical telemetry.

Example:

```csharp
double[][]
```

A jagged array is useful because different telemetry batches may contain different numbers of readings.

This reflects a realistic IoT environment where devices do not necessarily send the same amount of data during every collection period.

---

# 9. Generic Collections

Raw telemetry stored in arrays can be transferred into generic collections such as:

```csharp
List<double>
```

This allows telemetry to be processed more easily by the rest of the application.

Generic collections are also used throughout Smart-X to manage sensors, telemetry records, and uploaded file metadata.

---

# 10. Recursive Validation

Smart-X includes recursive validation of telemetry history.

The recursive validation method moves through the jagged telemetry structure and validates readings until the end of the telemetry history is reached.

A clear base condition is included to ensure recursion terminates correctly.

The validation process can detect invalid numerical values such as:

```text
NaN
Infinity
```

before the telemetry is used for further calculations.

---

# 11. Sensor File and Log Uploads

Users can attach files to registered sensor profiles.

Supported file types include:

```text
.txt
.log
.csv
.json
.jpg
.jpeg
.png
```

Examples include:

* Hardware diagnostic logs
* Device configuration files
* Deployment photographs
* Sensor export files

Files are uploaded using:

```text
multipart/form-data
```

The API validates:

* Sensor ID
* File extension
* File size
* File type

The maximum supported upload size is:

```text
5 MB
```

Files are assigned unique stored filenames to reduce the risk of filename collisions.

Uploads are grouped according to the sensor to which they belong.

---

# API Endpoints

The application exposes several API endpoints.

Examples include:

```text
GET /api/sensors

GET /api/sensors/{id}

POST /api/sensors

DELETE /api/sensors/{id}
```

Telemetry endpoints include:

```text
POST /api/Telemetry

POST /api/Telemetry/power

POST /api/Telemetry/actuator

GET /api/Telemetry

GET /api/Telemetry/sensor/{sensorId}

GET /api/Telemetry/history

GET /api/Telemetry/typed-summary

GET /api/Telemetry/aggregation-demo
```

File upload endpoints include:

```text
POST /api/uploads

GET /api/uploads/sensor/{sensorId}
```

Swagger can be used to test the available API endpoints.

---

# System Requirements

To run Smart-X, the following software is required:

* Visual Studio 2022 or later with .NET development tools
* .NET 10 SDK
* A modern web browser
* Git, if cloning the project from GitHub

The installed .NET SDK can be checked with:

```bash
dotnet --version
```

The available SDKs can be displayed with:

```bash
dotnet --list-sdks
```

---

# Setup Instructions

## 1. Clone the Repository

Clone the Smart-X repository using Git:

```bash
git clone <repository-url>
```

Move into the project directory:

```bash
cd SmartX
```

Alternatively, download the repository as a ZIP file and extract it.

---

# 2. Restore Dependencies

From the solution directory run:

```bash
dotnet restore
```

This restores the NuGet packages required by the solution.

---

# 3. Build the Solution

Run:

```bash
dotnet build
```

The application should build successfully with no compilation errors.

Visual Studio users can alternatively select:

```text
Build
→ Rebuild Solution
```

---

# 4. Configure the API Address

The SmartX.Client application communicates with SmartX.Api using `HttpClient`.

Open:

```text
SmartX.Client
→ Program.cs
```

Verify that the configured API address matches the HTTPS address used by SmartX.Api.

Example:

```csharp
client.BaseAddress =
    new Uri("https://localhost:7123/");
```

The exact port number may differ depending on the development environment.

The API address can be confirmed in:

```text
SmartX.Api
→ Properties
→ launchSettings.json
```

---

# 5. Run the Backend API

The API can be started from Visual Studio or from the command line.

From the root directory run:

```bash
dotnet run --project SmartX.Api
```

When the API starts successfully, Swagger should be available through the API development address.

Swagger can be used to test sensor registration and telemetry endpoints.

---

# 6. Run the Client Application

Open another terminal and run:

```bash
dotnet run --project SmartX.Client
```

The Smart-X web interface should open in the browser.

If it does not open automatically, use the HTTPS address displayed in the terminal.

---

# Running Both Projects in Visual Studio

When using Visual Studio, configure both projects to start together.

Right-click the solution and select:

```text
Configure Startup Projects
```

Select:

```text
Multiple startup projects
```

Set:

```text
SmartX.Api      Start
SmartX.Client   Start
```

Start the solution.

Both the backend API and client application must be running for the full Smart-X functionality to operate.

---

# Example Demonstration

## Register a Sensor

Example registration data:

```text
Device Name:
Hydroponic Temperature Sensor

MAC Address:
AA:BB:CC:DD:EE:01

Deployment Location:
Hydroponic Facility

Zone:
Zone A

Category:
Environmental
```

---

# Submit Decimal Telemetry

Use:

```text
POST /api/Telemetry
```

Example:

```json
{
  "sensorId": 1,
  "metricName": "Temperature",
  "value": 24.5,
  "unit": "C"
}
```

This produces a:

```csharp
TelemetryPacket<double>
```

---

# Submit Integer Telemetry

Use:

```text
POST /api/Telemetry/power
```

Example:

```json
{
  "sensorId": 1,
  "metricName": "Power",
  "value": 850,
  "unit": "W"
}
```

This produces a:

```csharp
TelemetryPacket<int>
```

---

# Submit Boolean Telemetry

Use:

```text
POST /api/Telemetry/actuator
```

Example:

```json
{
  "sensorId": 1,
  "metricName": "ValveState",
  "value": true
}
```

This produces a:

```csharp
TelemetryPacket<bool>
```

---

# Demonstrating Generic Telemetry

Execute:

```text
GET /api/Telemetry/typed-summary
```

The endpoint displays telemetry stored using the different generic data types.

---

# Demonstrating Operator Overloading

Execute:

```text
GET /api/Telemetry/aggregation-demo
```

The example combines two power readings:

```text
450 W + 350 W = 800 W
```

It also demonstrates threshold comparison using overloaded operators.

---

# Demonstrating Anomaly Detection

Submit a temperature value outside the normal range.

Example:

```json
{
  "sensorId": 1,
  "metricName": "Temperature",
  "value": 52.8,
  "unit": "C"
}
```

Smart-X identifies the reading as anomalous.

The sensor should then appear in a warning state on the dashboard.

---

# Demonstrating Telemetry History

Execute:

```text
GET /api/Telemetry/history
```

The response demonstrates:

* Jagged-array telemetry storage
* Recursive validation
* Conversion to a generic `List<T>`
* Telemetry processing

---

# Demonstrating File Upload

Navigate to:

```text
Sensors
→ Upload File
```

Select a supported log, configuration or image file.

The file is submitted to the API using multipart upload and linked to the selected sensor.

---

# Validation and Error Handling

Smart-X provides validation for common user and telemetry errors.

Examples include:

* Missing sensor information
* Incorrect MAC address structure
* Duplicate MAC addresses
* Unknown sensor IDs
* Unsupported uploaded files
* Files exceeding the configured size limit
* Abnormal telemetry values
* API connection failures

Where appropriate, error messages are returned to the user through the interface or API response.

---

# Testing the Application

Before running the application, verify that both projects compile successfully.

Recommended test sequence:

1. Start SmartX.Api.
2. Start SmartX.Client.
3. Register a sensor.
4. Verify the sensor appears in the sensor list.
5. Open the dashboard.
6. Submit normal telemetry.
7. Verify the sensor remains connected.
8. Submit anomalous telemetry.
9. Verify the sensor changes to a warning state.
10. Test integer power telemetry.
11. Test boolean actuator telemetry.
12. Run the typed telemetry summary.
13. Run the operator overloading demonstration.
14. Run the telemetry history endpoint.
15. Upload a sensor log or deployment image.
16. Verify the dashboard continues to operate correctly.

---

# Current Project Scope

This submission implements:

```text
Sensor Data Ingestion and Telemetry
```

The following Smart-X architectural pillars remain outside the current Part 1 implementation:

```text
Real-Time Command Stream and History

Network Topology and Mesh Routing
```

These modules are displayed in the user interface as disabled future functionality.

---

# Academic Concepts Demonstrated

The project demonstrates the following PROG7312 concepts:

* Object-oriented programming
* Generic classes
* Generic collections
* Operator overloading
* Jagged arrays
* Recursive methods
* Asynchronous programming
* API architecture
* REST communication
* Model validation
* File handling
* Multipart file uploads
* Separation of concerns
* Responsive user-interface design

---

# Project Status

Smart-X Part 1 currently provides a working simulation of an IoT telemetry gateway and technical management dashboard.

The system can register devices, ingest multiple telemetry data types, detect abnormal values, monitor sensor health, process historical telemetry and associate diagnostic files with individual sensors.

Future Smart-X development will extend the ecosystem with command-stream processing and network topology management.
