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
- React with Typescript via Vite
- Sass

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Node.js 18+ (for the frontend)

### Setup

1. Install frontend dependencies:
   ```bash
   cd TodoSpa
   npm install
   ```

### Running the Application

1. Start the API:
   ```bash
   cd TodoApi
   dotnet run
   ```

2. Start the frontend:
   ```bash
   cd TodoSpa
   npm run dev
   ```

3. Access the application:
   - Frontend: `http://localhost:5173`
   - API Documentation (Scalar UI): `https://localhost:7275/scalar`

**Note:** The frontend runs on HTTP for easier development (HMR works better), while the API runs on HTTPS. The CORS configuration allows this mixed setup.

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


## Design Decisions
- App is small with few endpoints, so minimal api is used. If the app was expanded, controllers could be used - potentially with a tool like MediatR.
- In-memory database is used. Easy for showcasing the app, and nice that it resets when you restart the dev server.
- The API has an in-memory caching layer for todos. It caches todos using a cache key on user id.
- The API uses JWT token authentication. This secures our Todo endpoints and prevents users from accessing Todos that do not belong to them.
- When we DELETE Todos, they are marked as IsDeleted and they will no longer be returned in GET requests. They still live in the database though, so we preserve all of our data.
- For the user signup, no matter what email you enter the message always says 'Please check your email inbox to verify your account.' this is important because it prevents user enumeration.
- The frontend uses a mobile friendly design so it is usable on all devices.
- The frontend uses sass with css modules for clean, isolated styling.
