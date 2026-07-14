# Sany3y - صنـايعـي

<div align="center">

![Sany3y Banner](docs/3ommal.png)

[Logo](https://img.shields.io/badge/Sany3y-Service_Marketplace-blue?style=for-the-badge)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-GPL--3.0-green?style=for-the-badge)](LICENSE)

**A modern service marketplace platform connecting clients with trusted technicians**

[Features](#-features) • [Tech Stack](#-technology-stack) • [Getting Started](#-getting-started) • [Documentation](#-documentation) • [Team](#-team-members)

</div>

---

## 📌 Overview

**3ommal** (عمال) is a comprehensive service marketplace platform built with **ASP.NET Core 8** that bridges the gap between clients seeking home or business maintenance services and skilled technicians. Whether you need an electrician, plumber, carpenter, or any other service professional, Sany3y provides a seamless, secure, and efficient platform for connecting and managing service requests.

### 🎯 Key Highlights

- 🔐 **Secure Authentication** - ASP.NET Core Identity with Google OAuth integration
- 💬 **Real-time Communication** - Live chat and notifications using SignalR
- 💳 **Flexible Payments** - Stripe integration for online payments + cash option
- 📅 **Smart Scheduling** - Technician availability management system
- ⭐ **Rating System** - Build trust through reviews and ratings
- 🌍 **Location Services** - Comprehensive Egyptian location hierarchy

---

## ✨ Features

### For Clients 👤

- ✅ Browse and search technicians by category, location, and rating
- ✅ View detailed technician profiles with experience and pricing
- ✅ Check real-time availability and book appointments
- ✅ Secure payment options (Cash on Service)
- ✅ Real-time chat with technicians
- ✅ Rate and review service providers
- ✅ Track booking history and status
- ✅ Receive instant notifications

### For Technicians 🔧

- ✅ Create professional profiles with portfolio
- ✅ Set custom pricing and service categories
- ✅ Manage availability schedule with time slots
- ✅ Receive and manage booking requests
- ✅ Direct communication with clients
- ✅ Track earnings and payment history
- ✅ Build reputation through ratings
- ✅ AI-verified identity for trust

### For Administrators 👨‍💼

- ✅ Comprehensive admin dashboard
- ✅ User and technician management
- ✅ Category and service oversight
- ✅ System health monitoring
- ✅ Content moderation tools

---

## 🏗️ Architecture

The solution follows a **clean 3-tier architecture** with clear separation of concerns:

```
Sany3y/
├── Sany3y/                      # Main Web Application (MVC)
│   ├── Controllers/             # 6 MVC Controllers
│   ├── Views/                   # Razor Views (Account, Admin, Dashboard, etc.)
│   ├── wwwroot/                 # Static files (CSS, JS, images)
│   ├── Extensions/              # Service configuration extensions
│   ├── Hubs/                    # SignalR hubs (Chat, UserStatus)
│   └── Program.cs               # Application entry point
│
├── Sany3y.API/                  # RESTful API Backend
│   ├── Controllers/             # 16 API Controllers
│   ├── Services/                # Business logic services
│   ├── DTOs/                    # Data Transfer Objects
│   └── Program.cs               # API entry point
│
├── Sany3y.Infrastructure/       # Data Access Layer
│   ├── Models/                  # 17 Entity models
│   ├── Repositories/            # 9 Repository implementations
│   ├── ViewModels/              # View models for data transfer
│   ├── Migrations/              # EF Core migrations
│   └── AppDbContext.cs          # Database context
│
└── docs/                        # Documentation
    ├── Business Requirements Specification.pdf
    └── ERDs/                    # Database diagrams
```

---

## 🔧 Technology Stack

### Backend
- **Framework**: .NET 8 (LTS)
- **Web Framework**: ASP.NET Core MVC
- **API**: ASP.NET Core Web API with Swagger
- **ORM**: Entity Framework Core 8
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity + JWT
- **Real-time**: SignalR for WebSocket communication

### Frontend
- **Template Engine**: Razor Pages
- **CSS Framework**: Bootstrap 5
- **JavaScript**: jQuery + Vanilla JS
- **Validation**: jQuery Validation

### Third-Party Integrations
- **OAuth**: Google Authentication
- **Email**: ASP.NET Core Email Service
- **PDF Generation**: iTextSharp

### DevOps & Tools
- **Version Control**: Git & GitHub
- **CI/CD**: GitHub Actions (.github/workflows/dotnet.yml)
- **API Documentation**: Swagger/OpenAPI
- **Package Manager**: NuGet

---

## 📊 Database Schema

### Core Entities

| Entity | Description |
|--------|-------------|
| **User** | Extended IdentityUser with custom properties (NationalId, Bio, etc.) |
| **Category** | Service categories (Electrician, Plumber, Carpenter, etc.) |
| **Task** | Service bookings connecting clients and technicians |
| **Rating** | Reviews and ratings for technicians |
| **Message** | Real-time chat messages |
| **Notification** | User notifications |
| **TechnicianSchedule** | Availability time slots |
| **Address** | User location information |
| **ProfilePicture** | User profile images |

### Location Hierarchy
- **Province** → **Governorate** → **City** (Egyptian administrative divisions)

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/khaled22205/3OMAL-Platform.git
   cd Sany3y
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string**
   
   Edit `Sany3y/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Sany3yDB;Trusted_Connection=True;"
     }
   }
   ```

4. **Configure API settings**
   
   Edit `Sany3y.API/appsettings.json` with your Stripe keys:
   ```json
   {
     "Stripe": {
       "SecretKey": "your_stripe_secret_key",
       "PublishableKey": "your_stripe_publishable_key"
     }
   }
   ```

5. **Apply database migrations**
   ```bash
   cd Sany3y
   dotnet ef database update
   ```

6. **Run the application**
   
   **Option 1: Run both projects simultaneously (recommended)**
   ```bash
   # Terminal 1 - API
   cd Sany3y.API
   dotnet run
   
   # Terminal 2 - Web App
   cd Sany3y
   dotnet run
   ```
   
   **Option 2: Using Visual Studio**
   - Right-click solution → Properties → Multiple Startup Projects
   - Set both `Sany3y` and `Sany3y.API` to "Start"

7. **Access the application**
   - Web App: `https://localhost:7001` (or check console output)
   - API: `https://localhost:7178`
   - Swagger: `https://localhost:7178/swagger`

### Python ML Setup (Optional)

For ID verification features:

```bash
cd Sany3y.API/py
pip install -r requirements.txt
python app.py
```

---

## 📖 Documentation

### Project Documents
- 🗺️ [Physical ERD Diagram](./docs/ERDs/ERD.png)
- 📄 [Full Documentation](./docs/Sany3y.pdf)

### API Documentation
- Access Swagger UI at `https://localhost:7178/swagger` when running the API

### Key Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/Category/GetAll` | GET | Get all service categories |
| `/api/Technician/GetAll` | GET | Get all technicians |
| `/api/TechnicianSchedule/GetAvailability` | GET | Get technician availability |
| `/api/Payment/CreateStripeSession` | POST | Create Stripe payment session |
| `/api/Message/Send` | POST | Send chat message |
| `/api/Notification/GetUserNotifications` | GET | Get user notifications |

---

## 🔐 Authentication Flow

1. **Registration** → Email verification → Profile completion
2. **Login** → Session creation → Dashboard redirect
3. **Google OAuth** → External authentication → Profile linking
4. **JWT Tokens** → API authentication for mobile/external clients

---

## 💡 Usage Examples

### Booking a Service

1. Browse available technicians by category
2. View technician profile and ratings
3. Check availability calendar
4. Select time slot and create booking
5. Choose payment method (Stripe/Cash)
6. Receive confirmation and chat with technician

### Managing Technician Profile

1. Register as technician
2. Complete profile with category and pricing
3. Upload ID for verification (AI-powered)
4. Set availability schedule
5. Receive booking requests
6. Manage appointments and earnings

---

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Style
- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Write unit tests for new features

---

## 📝 License

This project is licensed under the **GNU General Public License v3.0** - see the [LICENSE](LICENSE) file for details.

---
---

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Stripe for payment processing
- SignalR for real-time capabilities
- Bootstrap for responsive UI components
- All open-source contributors

---

<div align="center">

**Built with ❤️ by the Sany3y Team**

⭐ Star us on GitHub if you find this project useful!

</div>
