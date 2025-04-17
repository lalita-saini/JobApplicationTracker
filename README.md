# Job Application Tracker

A .NET Core Web API application for tracking job applications with user authentication and authorization.

## Table of Contents
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Database Setup](#database-setup)
- [API Documentation](#api-documentation)
- [Authentication](#authentication)
- [Testing](#testing)
- [Project Structure](#project-structure)

## Features
- User authentication and authorization using JWT tokens
- CRUD operations for job applications
- SQLite database for data persistence
- Input validation and error handling
- Swagger API documentation

## Prerequisites
- .NET 7.0 SDK or later
- Visual Studio 2022 or VS Code
- SQLite (included in the project)

## Getting Started

1. Clone the repository:
```bash
git clone [https://github.com/yourusername/JobApplicationTracker.git](https://github.com/lalita-saini/JobApplicationTracker.git)
cd JobApplicationTracker
```

2. Install dependencies:
```bash
dotnet restore
```

3. Update the database:
```bash
# Install the EF Core CLI tools if you haven't already
dotnet tool install --global dotnet-ef

# Create initial migration in case there is no migrations. You will not need it as there are already migrations added to it
dotnet ef migrations add InitialCreate

# Apply migrations to create/update the database
dotnet ef database update
```

4. Run the application:
```bash
dotnet run
```

The API will be available at `https://localhost:7286` (HTTPS) or `http://localhost:5000` (HTTP)

## Database Setup

The project uses SQLite as the database. The database file (`jobapplications.db`) will be created automatically when you run the migrations.

### Managing Migrations

Create a new migration:
```bash
dotnet ef migrations add [MigrationName]
```

Apply migrations:
```bash
dotnet ef database update
```

### Database Schema
The main entities in the database are:

1. Users
```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

2. Job Applications
```csharp
public class JobApplication
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string Position { get; set; }
    public string Status { get; set; }
    public DateTime DateApplied { get; set; }
    public string Notes { get; set; }
    public int UserId { get; set; }
}
```

## API Documentation

The API documentation is available through Swagger UI at `http://localhost:5000/swagger/index.html` when running the application.

### Main Endpoints

#### Authentication
- POST `/api/auth/register` - Register a new user
- POST `/api/auth/login` - Login and get JWT token

#### Job Applications
- GET `/api/jobapplications` - Get all applications for the current user
- GET `/api/jobapplications/{id}` - Get a specific application
- POST `/api/jobapplications` - Create a new application
- PUT `/api/jobapplications/{id}` - Update an application

## Authentication

The API uses JWT (JSON Web Token) for authentication. To access protected endpoints:

1. Register a new user or login to get a token
2. Include the token in the Authorization header:
