# ITPE3200 Exam Project

This project consists of:

- **Backend**: ASP.NET Core 8 Web API (`api` folder)
  - Uses **Entity Framework Core** with **SQLite** (`AppDatabase.db`, `AuthDatabase.db`)
  - Includes authentication/authorization, appointments, clients, healthcare workers, and available slots
- **Frontend**: React + TypeScript + Vite (`frontend` folder)
- **Tests**: xUnit test project for the API (`api.Tests` folder)
  - Uses **Moq** and **Moq.EntityFrameworkCore** for mocking dependencies

## Prerequisites

**Node version**: 22.11.0

**.NET version**: 8.0

## 1. Running the Backend (API)

1. Open a terminal in the `api` folder:

   ```bash
   cd api
   ```

2. Restore dependencies
   ```bash
   dotnet restore
   ```
3. Run the API:
   ```bash
   dotnet run
   ```

## 1. Running the Frontend

1. Open a new terminal in the `frontend` folder:

   ```bash
   cd frontend
   ```

2. Install dependencies
   ```bash
   npm install
   ```
3. Run the Frontend:
   ```bash
   npm run dev
   ```
4. Vite will show a local URL, typically:
   ```bash
   http://localhost:5173/
   ```
   Open this URL in your browser.
   Make sure the backend is running so the frontend can talk to the API.

## Login, Authentication and Authorization

Below are all the pre-seeded user accounts created by the database initializer (`DBInit` and `AuthDbInit`).

> **Note:** If the database files are not present, or if the seeding flag in appsettings.Development.json is set to true, the databases will be reseeded on startup.

#### User Logins

| Name        | Email                    | Password       | Role             |
| ----------- | ------------------------ | -------------- | ---------------- |
|             | **admin@homecare.local** | **Admin123!**  | Admin            |
| John Doe    | **john@homecare.local**  | **Client123!** | Client           |
| Alice Brown | **alice@homecare.local** | **Worker123!** | HealthcareWorker |

## Running Test (Backend)

1. From the `api.Tests` folder:

   ```bash
   cd api.Tests
   ```

2. Restore dependencies
   ```bash
   dotnet restore
   ```
3. Run tests:
   ```bash
   dotnet test
   ```
