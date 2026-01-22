# Book Management API

A REST API for managing books, authors, and illustrators built with .NET 10.0, Entity Framework Core, and SQL Server.

## 📋 Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Quick Start with Docker](#quick-start-with-docker)
- [Local Development Setup](#local-development-setup)
- [Running Tests](#running-tests)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Validation Rules](#validation-rules)

---

## ✨ Features

- ✅ Create and list books with full validation
- ✅ Filter books by author
- ✅ Sort books by title, publication year, or genre
- ✅ Comprehensive validation rules (7 business rules)
- ✅ Unit tests with xUnit
- ✅ Swagger/OpenAPI documentation
- ✅ Entity Framework Core with SQL Server
- ✅ Docker support for easy deployment
- ✅ Clean Architecture pattern

---

## 🔧 Prerequisites

### Option 1: Using Docker (Recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (for local testing)

### Option 2: Local Development
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) or Docker for SQL Server

---

## 🐳 Quick Start with Docker

### 1. Using Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/SABBIDO7/books_app.git
cd books_app

# Start both SQL Server and the API
docker compose up --build -d

# Check if containers are running
docker compose ps
```

The API will be available at:
- **HTTP**: http://localhost:5034
- **HTTPS**: https://localhost:7265
- **Swagger UI**: http://localhost:5034/swagger

**Notes**:
- This Docker setup runs the API over HTTP only on port 5034 (mapped to container port 8080).
- SQL Server is started in Docker and persists data in a Docker volume unless you remove volumes.

### 2. Run Tests with Docker
- Open new terminal and run the tests like this:

```bash
docker compose run --rm tests dotnet test -c Release
```

### 3. Stop the Application

```bash
docker-compose down

# To remove volumes (database data)
docker-compose down -v
```

---

## 💻 Local Development Setup

### Step 1: Start SQL Server

```bash
docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=YourStrong!Passw0rd' \
  -p 1433:1433 --name books-sqlserver --hostname books-sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**Note**: If you have already run this command before and the container is currently stopped, you only need to start it using:
```bash
docker start books-sqlserver
```

### Step 2: Navigate to Project Directory

```bash
cd books_app
```

### Step 3: Restore Dependencies

```bash
dotnet restore
```

### Step 4: Run Database Migrations

**Option A: Automatic (migrations run on app startup)**
```bash
dotnet run
```

**Option B: Manual**
```bash
# Create migration (if not exists)
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

# Apply migration to database
dotnet ef database update
```

### Step 5: Run the Application

```bash
dotnet run
```

The API will be available at:
- **Swagger UI**: http://localhost:5034/swagger
- **HTTP**: http://localhost:5034
- **HTTPS**: https://localhost:7265

---

## 🧪 Running Tests

### Run All Tests

```bash
# From the project root
dotnet test
```

### Run Specific Test

```bash
dotnet test --filter "FullyQualifiedName~ValidateAsync_TitleIsMissing_ReturnsError"
```

### Test Coverage

The project includes 14 unit tests covering:
- ✅ All 7 validation rules
- ✅ Edge cases (books before 1970, valid books)
- ✅ Error scenarios (non-existent authors/illustrators)
- ✅ Duplicate detection (ISBN and book combinations)

---

## 📚 API Documentation

### Endpoints

#### 1. Create a Book
```http
POST /api/books
Content-Type: application/json

{
  "title": "The Shining",
  "publicationYear": 1977,
  "authorIds": [1],
  "illustratorId": 1,
  "genres": [3, 4],
  "isbn": "9780385121675"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "title": "The Shining",
  "publicationYear": 1977,
  "authors": ["Stephen King"],
  "illustrator": "Norman Rockwell",
  "genres": ["Drama", "Horror"],
  "isbn": "9780385121675"
}
```

#### 2. List All Books
```http
GET /api/books (http://localhost:5034/api/books)
```

#### 3. Filter Books by Author
```http
GET /api/books?authorId=1 (http://localhost:5034/api/books?authorId=1)
```

#### 4. Sort Books
```http
GET /api/books?sortBy=title (http://localhost:5034/api/books?sortBy=title)
GET /api/books?sortBy=year (http://localhost:5034/api/books?sortBy=year)
GET /api/books?sortBy=genre (http://localhost:5034/api/books?sortBy=genre)
```

### Genre Values

| Value | Genre |
|-------|-------|
| 1 | Action |
| 2 | Comedy |
| 3 | Drama |
| 4 | Horror |
| 5 | ScienceFiction |

### Pre-seeded Data

**Authors:**
- ID 1: Stephen King
- ID 2: Marguerite Duras
- ID 3: Isaac Asimov

**Illustrators:**
- ID 1: Norman Rockwell
- ID 2: Gustave Doré
- ID 3: Beya Rebaï

---

## 📁 Project Structure

```
books_app/
├── Application/
│   ├── Dtos/                    # Data Transfer Objects
│   │   ├── BookResponseDto.cs
│   │   ├── CreateBookDto.cs
│   │   └── ValidationResultDto.cs
│   ├── enums/
│   │   └── Genre.cs
│   ├── Services/
│   │   └── BookService.cs       # Business logic
│   └── Validators/
│       └── BookValidator.cs     # Validation rules
│
├── Domain/
│   └── Entities/                # Domain models
│       ├── Author.cs
│       ├── Book.cs
│       ├── BookAuthor.cs
│       ├── BookGenre.cs
│       └── Illustrator.cs
│
├── Infrastructure/
│   └── Data/
│       └── BookDbContext.cs     # EF Core context
│
├── controller/
│   └── BooksController.cs       # API endpoints
│
├── Tests/
│   └── BookValidatorTests.cs    # Unit tests
│
├── Data/
│   └── Migrations/              # EF Core migrations
│
├── Program.cs                   # Application entry point
├── appsettings.json            # Configuration
├── Dockerfile                   # Docker configuration
├── docker-compose.yml          # Docker Compose setup
└── README.md
```

---

## 🏗️ Architecture

### Clean Architecture Layers

1. **Domain Layer** (`Domain/Entities/`)
   - Pure business entities
   - No dependencies on other layers
   - Contains: Book, Author, Illustrator, etc.

2. **Infrastructure Layer** (`Infrastructure/Data/`)
   - Database context (EF Core)

3. **Application Layer** (`Application/`)
   - Business logic (Services)
   - Validation (Validators)
   - DTOs for data transfer

4. **API Layer** (`controller/`)
   - REST API endpoints
   - HTTP request/response handling
   - Dependency injection setup

---

## ✅ Validation Rules

### 1. Title is Mandatory
Books must have a non-empty title.

### 2. Illustrator is Mandatory
Books must have an illustrator with a valid ID.

### 3. No Future Publication Years
Publication year cannot be greater than the current year.

### 4. No Books Before 1450
Publication year must be 1450 or later.

### 5. ISBN Requirements
- **Books before 1970**: ISBN is optional
- **Books from 1970+**: ISBN is required and must be exactly 13 digits
- ISBN must contain only numeric characters

### 6. Unique ISBN
No two books can have the same ISBN.

### 7. No Duplicate Books
No two books can have the same combination of:
- Title
- Publication Year
- Author

---

## 🔍 Testing the API

### Using Swagger UI

1. Navigate to http://localhost:5034/swagger
2. Expand the `POST /api/books` endpoint
3. Click "Try it out"
4. Enter the request body
5. Click "Execute"

### Using cURL

```bash
# Create a book
curl -X POST http://localhost:5034/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Foundation",
    "publicationYear": 1951,
    "authorIds": [3],
    "illustratorId": 2,
    "genres": [5],
    "isbn": "9780553293357"
  }'

# List all books
curl http://localhost:5034/api/books

# Filter by author
curl http://localhost:5034/api/books?authorId=1

# Sort by title
curl http://localhost:5034/api/books?sortBy=title
```

### Example Validation Errors

**Invalid Request (future year):**
```json
{
  "type": "ValidationError",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": [
    "Publication year cannot be in the future (current year: 2026)."
  ]
}
```

---

## 🐛 Troubleshooting

### SQL Server Connection Issues

**Problem**: Cannot connect to SQL Server
```
Solution:
1. Check if SQL Server container is running:
   docker ps | grep books-sqlserver

2. Verify connection string in appsettings.json

3. Ensure port 1433 is not in use:
   lsof -i :1433
```

### Migration Issues

**Problem**: Migration fails or database not created
```
Solution (After navigating to the `books_app`):
1. Delete existing migrations:
   rm -rf Data/Migrations

2. Recreate migrations:
   dotnet ef migrations add InitialCreate --output-dir Data/Migrations

3. Apply migrations:
   dotnet ef database update
```

### Port Already in Use

**Problem**: Port 5034 already in use
```
Solution:
1. Find process using the port:
   lsof -i :5034

2. Kill the process:
   kill -9 <PID>

3. Or change port in launchSettings.json
```

---

## 📝 Additional Notes

### Libraries Not Used (As Per Test Requirements)

   > Microsoft.EntityFrameworkCore.Design
   > Microsoft.EntityFrameworkCore.InMemory
   > Microsoft.EntityFrameworkCore.SqlServer
   > Microsoft.NET.Test.Sdk
   > Swashbuckle.AspNetCore
   > xunit
   > xunit.runner.visualstudio


### Future Enhancements

- [ ] Add pagination for book listing
- [ ] Implement soft deletes
- [ ] Add book update and delete endpoints
- [ ] Add full-text search

---

## 📄 License

This project is for technical assessment purposes.

---

**Author**: Walid Sabbidine  
**Date**: January 2026  
**Framework**: .NET 10.0  
**Database**: SQL Server 2022