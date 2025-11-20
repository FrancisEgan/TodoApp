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
- xUnit tests
- Scalar for OpenAPI documentation
- React with Typescript via Vite
- Sass
- React Query
- Vitest unit tests

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Node.js 25 or later

### Setup

1. Install frontend dependencies:
   ```bash
   cd TodoSpa
   npm install
   ```

2. Trust the development certificate:
   ```bash
   dotnet dev-certs https --trust
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

4. Creating a user:
   - When you enter an email address in the signup, it will instruct you to check the API console for the verification link. Emailing is not implemented in this app, so you will find the link that would normally be found in your email in the console where you are running the API.

**Note:** The frontend runs on HTTP for easier development (HMR works better), while the API runs on HTTPS. The CORS configuration allows this mixed setup.

## API Endpoints

### Authentication
- `POST /auth/signup` - Create a new user account (prints token in console)
- `POST /auth/verify` - Verify account with token and set name/password
- `POST /auth/login` - Login and receive JWT token
- `POST /auth/resend-verification` - Resend verification email (prints token in console)

### Todos (Requires Authentication)
- `GET /todos` - Get all todos for the authenticated user
- `GET /todos/{id}` - Get a specific todo
- `POST /todos` - Create a new todo
- `PUT /todos/{id}` - Update a todo (supports partial updates)
- `DELETE /todos/{id}` - Delete a todo (soft delete)

## Assumptions
- Todos can be created, deleted, and edited
- Users will want to be able to use the app from their mobile device
- Users should not see each others todos
- The app is for individual use cases, not team/organizational use

## Design Decisions
- App is small with few endpoints, so minimal api is used. If the app was expanded, controllers could be used - potentially with a tool like MediatR.
- In-memory database is used. Easy for showcasing the app, and nice that it resets when you restart the dev server.
- The API has an in-memory caching layer for todos. It caches todos using a cache key on user id.
- The API uses JWT token authentication. This secures our Todo endpoints and prevents users from accessing Todos that do not belong to them.
- When we DELETE Todos, they are marked as IsDeleted and they will no longer be returned in GET requests. They still live in the database though, so we preserve all of our data.
- The API uses xUnit to create modern simple unit tests.
- For the user signup, no matter what email you enter the message always says 'Please check your email inbox to verify your account.' this is important because it prevents user enumeration.
- The frontend uses React Query to add another caching layer that lives on the frontend, making the app even more performant - and allows the code to be much cleaner without now having to handle loading states.
- The frontend uses a mobile friendly design so it is usable on all devices.
- The frontend uses sass with css modules for clean, isolated styling.
- The frontend uses vitest for unit testing.

## Trade offs
- In memory database - persistent storage would allow us to continue where we left off when re-running the app, but this app is for demo purposes only.
- No pagination - we could improve the app further with pagination, but we want to keep things simple for now
- Soft deletes - we preserve the data and could allow for undo operations, but this increases the database size
- JWT token in localStorage instead of httpOnly - we need to do it this way to run the app locally in HTTP
- Optimistic UI updates in react query - better UX but we have to rollback if anything goes wrong
- Minimal API - clean for this small project. Could be expanded to controllers for a larger app
- Single page application - server side rendering could improve initial load, but it was requested that this app be made in react.

## Extending the app for production and scaling
- A dedicated database should be used instead of in-memory
- In-memory cache should be moved to a distributed cache (Redis or similar)
- An e-mail provider should be hooked up so users can be created properly via email verification
- Signup via popular providers like Google or Microsoft accounts would improve the user signup flow significantly
- Further improve JWT functionality by adding refresh tokens
- Add pagination for todo lists to improve performance
- The app could be containerized with Docker or similar to facilitate cloud deployment
- Testing could be expanded to include integration tests
- Privacy policy and terms of service are a must for a production app
- The app needs token revocation so that if a user has a stale browser session they are logged out
