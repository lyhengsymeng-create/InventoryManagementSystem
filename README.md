# Inventory Management System

A full-stack inventory management application built with **Angular**,
**ASP.NET Core Web API**, and **PostgreSQL**. The backend uses Entity
Framework Core for data access, and the test project uses xUnit for
automated integration testing.

## Tech Stack

### Frontend

-   Angular
-   TypeScript
-   HTML and CSS

### Backend

-   C#
-   ASP.NET Core Web API (.NET 10)
-   Entity Framework Core
-   PostgreSQL
-   REST API

### Testing

-   xUnit integration tests
-   Postman for manual API testing

## Project Structure

``` text
InventoryManagementSystem/
├── backend/          # ASP.NET Core Web API
├── backend.Tests/    # Automated backend integration tests
├── frontend/         # Angular application
└── .gitignore
```

## Prerequisites

-   .NET 10 SDK
-   Node.js and npm (use a version compatible with the Angular project)
-   PostgreSQL
-   Git

## Configuration

### Configure the backend database

Configure the PostgreSQL connection string using the application's
supported configuration or environment-variable approach. Do not commit
real database passwords, access tokens, or other secrets to source
control.

For integration tests, set the environment variable:

``` text
INVENTORY_TEST_CONNECTION_STRING
```

Use a dedicated test database. The test setup is intended to run
migrations only when the database name is exactly:

``` text
inventory_management_integration_test
```

Do not point integration tests at a production or important development
database.

> Keep connection strings and credentials out of this README and out of
> Git history.

## Run the Backend

From the project root, run:

``` powershell
cd backend
dotnet restore
dotnet build
dotnet run
```

Use the URL printed by the application in the terminal. The actual URL
and port depend on the local launch configuration.

## Run the Frontend

Open a separate terminal:

``` powershell
cd frontend
npm install
npm start
```

If the `start` script is not defined in `frontend/package.json`, inspect
the available scripts and use the Angular serve script configured by the
project, commonly:

``` powershell
npx ng serve
```

Open the local URL printed by Angular in the terminal.

## Run Automated Tests

From the project root:

``` powershell
dotnet test backend.Tests
```

The integration tests require a valid `INVENTORY_TEST_CONNECTION_STRING`
pointing to the dedicated test database.

## Manual API Testing

Use Postman to send requests to the backend API. Use the base URL
printed when the backend starts, and verify the response status code and
response body for each endpoint.

## Security Notes

-   Never commit real database credentials, API keys, JWT secrets, or
    access tokens.
-   Keep local secrets in environment variables or an appropriate local
    secret store.
-   Use a separate database for integration tests.
-   Rotate any credential that may have been exposed previously.
-   Review `.gitignore` and configuration files before pushing changes
    to a public repository.

## Current Test Status

The project previously completed **5 automated tests successfully** in
the local development environment. Test results may vary after code or
environment changes; run the test command above to verify the current
state.

## Future Improvements

-   Document implemented API endpoints and example requests.
-   Add screenshots of the Angular interface.
-   Provide development and deployment configuration examples without
    secrets.
-   Add CI checks for builds and automated tests.

## Author

**Lyheng Symeng**

Computer Science student at the Royal University of Phnom Penh.
