# TodoApp

A secure todo list API built with ASP.NET Core that allows users to manage their personal tasks.

## Features

- **User Authentication**: JWT-based authentication with signup and login
- **Secure Todo Management**: Users can only view and manage their own todos
- **Caching**: In-memory caching for improved performance
- **OpenAPI Documentation**: Interactive API documentation with Scalar UI
- **Soft Deletes**: Todos are marked as deleted rather than permanently removed

## Tech Stack

- ASP.NET Core (Minimal APIs)
- Entity Framework Core (In-Memory Database)
- JWT Authentication
- Scalar for OpenAPI documentation

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later

### Running the Application

1. Navigate to the API directory:
   ```bash
   cd TodoApi
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Access the API documentation:
   - Scalar UI: `https://localhost:<port>/scalar/v1`
   - OpenAPI JSON: `https://localhost:<port>/openapi/v1.json`

## API Endpoints

### Authentication
- `POST /auth/signup` - Create a new user account
- `POST /auth/login` - Login and receive JWT token
- `POST /auth/set-password` - Set password for new users

### Todos (Requires Authentication)
- `GET /todos` - Get all todos for the authenticated user
- `GET /todos/{id}` - Get a specific todo
- `POST /todos` - Create a new todo
- `PUT /todos/{id}` - Update a todo
- `DELETE /todos/{id}` - Delete a todo (soft delete)
