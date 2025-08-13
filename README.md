# NiceDentist Manager API

Management system for dental clinic, responsible for customer, dentist, and appointment management.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with the following layers:

- **Domain**: Business entities (Customer, Dentist, Appointment)
- **Application**: Business rules and services (CustomerService, DentistService, AppointmentService)
- **Infrastructure**: Data access and external services implementations
- **API**: Controllers and Web API configuration

## 🔄 Event-Driven Integration

The Manager API integrates with the Auth API through RabbitMQ events:

### Event Flow:
1. **Manager API** creates Customer/Dentist → Publishes event to **Auth API**
2. **Auth API** creates user account → Publishes event back to **Manager API**
3. **Manager API** receives event and links UserId with Customer/Dentist

### Events:
- `CustomerCreated` → Sent to Auth API
- `DentistCreated` → Sent to Auth API  
- `UserCreated` → Received from Auth API

## 🛠️ Technologies

- **.NET 8**
- **ASP.NET Core Web API**
- **SQL Server** (via ADO.NET)
- **RabbitMQ** (Event-driven communication)
- **xUnit** (Unit and integration tests)
- **FluentAssertions** (Test assertions)
- **Moq** (Mocking framework)

## 📋 Features

### Customers
- ✅ Customer registration
- ✅ Customer listing
- ✅ Search by ID/email
- ✅ Data updates
- ✅ Customer deactivation

### Dentists
- ✅ Dentist registration
- ✅ Dentist listing
- ✅ Search by ID/email/license
- ✅ Data updates
- ✅ Dentist deactivation

### Appointments
- ✅ Appointment creation
- ✅ List by customer/dentist/date
- ✅ Status updates
- ✅ Appointment cancellation

### User Management
- ✅ Event-driven integration with Auth API
- ✅ Automatic UserId linking with Customer/Dentist
- ✅ Account creation notifications

## 🗄️ Database

### Tables:
- **Customers**: Customer data
- **Dentists**: Dentist data  
- **Appointments**: Appointments between customers and dentists

### Integration Fields:
- `Customers.UserId` → Reference to `Users.Id` in Auth DB
- `Dentists.UserId` → Reference to `Users.Id` in Auth DB

## 🚀 How to Run

### Prerequisites:
- .NET 8 SDK
- SQL Server (via Docker or local)
- RabbitMQ (via Docker or local)

### 1. Infrastructure:
```bash
# Start SQL Server + RabbitMQ
cd ../NiceDentist-Database
docker-compose up -d
```

### 2. Configuration:
```bash
# Configure connection string in appsettings.json
"ConnectionStrings": {
  "ManagerDb": "Server=localhost;Database=NiceDentistManagerDb;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True;"
}

# Configure RabbitMQ in appsettings.json
"RabbitMQ": {
  "HostName": "localhost",
  "UserName": "nicedentist",
  "Password": "Your_rabbit_password123!"
}
```

### 3. Run:
```bash
dotnet run --project src/NiceDentist.Manager.Api
```

### 4. Swagger:
Access: `https://localhost:5001/swagger`

## 🧪 Tests

### Run all tests:
```bash
dotnet test
```

### Run with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📁 Project Structure

```
NiceDentist-Manager/
├── src/
│   ├── NiceDentist.Manager.Domain/          # Entities
│   ├── NiceDentist.Manager.Application/      # Services and contracts
│   ├── NiceDentist.Manager.Infrastructure/   # Repositories and RabbitMQ
│   └── NiceDentist.Manager.Api/             # Controllers and configuration
├── tests/
│   ├── NiceDentist.Manager.Tests/           # Unit tests
│   └── NiceDentist.Manager.IntegrationTests/ # Integration tests
├── NiceDentist.Manager.sln
├── .gitignore
└── README.md
```

## 🔗 Related Repositories

- **Auth API**: [NiceDentist-AuthAPI](../NiceDentist-AuthAPI) - Authentication and authorization system
- **Database**: [NiceDentist-Database](../NiceDentist-Database) - SQL scripts and Docker Compose

## 📝 License

This project is part of a dental clinic demonstration system.

---

**NiceDentist Manager API** - Dental Management System 🦷
