# NiceDentist Manager API

Sistema de gerenciamento para clínica odontológica, responsável pelo cadastro e gerenciamento de clientes, dentistas e agendamentos.

## 🏗️ Arquitetura

Este projeto segue os princípios da **Clean Architecture** com as seguintes camadas:

- **Domain**: Entidades de negócio (Customer, Dentist, Appointment)
- **Application**: Regras de negócio e serviços (CustomerService, DentistService, AppointmentService)
- **Infrastructure**: Implementações de acesso a dados e serviços externos
- **API**: Controllers e configuração da Web API

## 🔄 Integração Event-Driven

O Manager API se integra com o Auth API através de eventos RabbitMQ:

### Fluxo de Eventos:
1. **Manager API** cria Customer/Dentist → Publica evento para **Auth API**
2. **Auth API** cria conta de usuário → Publica evento de volta para **Manager API**
3. **Manager API** recebe evento e linka UserId com Customer/Dentist

### Eventos:
- `CustomerCreated` → Enviado para Auth API
- `DentistCreated` → Enviado para Auth API  
- `UserCreated` → Recebido do Auth API

## 🛠️ Tecnologias

- **.NET 8**
- **ASP.NET Core Web API**
- **SQL Server** (via ADO.NET)
- **RabbitMQ** (Event-driven communication)
- **xUnit** (Testes unitários e integração)
- **FluentAssertions** (Assertions para testes)
- **Moq** (Mocking framework)

## 📋 Funcionalidades

### Customers (Clientes)
- ✅ Cadastro de clientes
- ✅ Listagem de clientes
- ✅ Busca por ID/email
- ✅ Atualização de dados
- ✅ Desativação de clientes

### Dentists (Dentistas)
- ✅ Cadastro de dentistas
- ✅ Listagem de dentistas
- ✅ Busca por ID/email/CRO
- ✅ Atualização de dados
- ✅ Desativação de dentistas

### Appointments (Agendamentos)
- ✅ Criação de agendamentos
- ✅ Listagem por cliente/dentista/data
- ✅ Atualização de status
- ✅ Cancelamento de agendamentos

### User Management (Gerenciamento de Usuários)
- ✅ Integração event-driven com Auth API
- ✅ Linking automático de UserId com Customer/Dentist
- ✅ Notificações de criação de conta

## 🗄️ Banco de Dados

### Tabelas:
- **Customers**: Dados dos clientes
- **Dentists**: Dados dos dentistas  
- **Appointments**: Agendamentos entre clientes e dentistas

### Campos de Integração:
- `Customers.UserId` → Referência para `Users.Id` no Auth DB
- `Dentists.UserId` → Referência para `Users.Id` no Auth DB

## 🚀 Como Executar

### Pré-requisitos:
- .NET 8 SDK
- SQL Server (via Docker ou local)
- RabbitMQ (via Docker ou local)

### 1. Infraestrutura:
```bash
# Subir SQL Server + RabbitMQ
cd ../NiceDentist-Database
docker-compose up -d
```

### 2. Configuração:
```bash
# Configurar connection string no appsettings.json
"ConnectionStrings": {
  "ManagerDb": "Server=localhost;Database=NiceDentistManagerDb;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True;"
}

# Configurar RabbitMQ no appsettings.json
"RabbitMQ": {
  "HostName": "localhost",
  "UserName": "nicedentist",
  "Password": "Your_rabbit_password123!"
}
```

### 3. Executar:
```bash
dotnet run --project src/NiceDentist.Manager.Api
```

### 4. Swagger:
Acesse: `https://localhost:5001/swagger`

## 🧪 Testes

### Executar todos os testes:
```bash
dotnet test
```

### Executar com cobertura:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📁 Estrutura do Projeto

```
NiceDentist-Manager/
├── src/
│   ├── NiceDentist.Manager.Domain/          # Entidades
│   ├── NiceDentist.Manager.Application/      # Serviços e contratos
│   ├── NiceDentist.Manager.Infrastructure/   # Repositórios e RabbitMQ
│   └── NiceDentist.Manager.Api/             # Controllers e configuração
├── tests/
│   ├── NiceDentist.Manager.Tests/           # Testes unitários
│   └── NiceDentist.Manager.IntegrationTests/ # Testes de integração
├── NiceDentist.Manager.sln
├── .gitignore
└── README.md
```

## 🔗 Repositórios Relacionados

- **Auth API**: [NiceDentist-AuthAPI](../NiceDentist-AuthAPI) - Sistema de autenticação e autorização
- **Database**: [NiceDentist-Database](../NiceDentist-Database) - Scripts SQL e Docker Compose

## 📝 Licença

Este projeto é parte de um sistema de demonstração para clínica odontológica.

---

**NiceDentist Manager API** - Sistema de Gerenciamento Odontológico 🦷
