# User Directory

A .NET 8 User Directory API designed for a separately developed React frontend. It provides authenticated CRUD operations for users, SQLite persistence, Swagger documentation, and Keycloak OAuth2/OIDC JWT validation.

## Structure

- `UserDirectory.API` - HTTP controllers, Swagger, CORS, authentication, and application composition.
- `UserDirectory.Application` - user DTOs, validation, use cases, and repository abstractions.
- `UserDirectory.Domain` - the `User` entity and domain behavior.
- `UserDirectory.Infrastructure` - EF Core SQLite DbContext, repository, and dependency registration.
- `UserDirectory.Tests` - application service unit tests.

The dependency direction points inward: API and Infrastructure depend on Application, Application depends on Domain, and Domain has no project dependencies.

## Run locally

Install the .NET 8 SDK and runtime, then run:

```powershell
dotnet restore user-directory-dotnet.slnx
dotnet run --project UserDirectory.API/UserDirectory.API.csproj
```

Swagger is available at `https://localhost:7064/swagger` or `http://localhost:5296/swagger`. The SQLite file defaults to `./data/app.db` in Development and `/data/app.db` in the container. The database directory is created by the application when it starts.

The anonymous readiness endpoint is `GET /health`.

## Run the complete stack with Docker

The single root `Dockerfile` builds both the .NET API and React frontend. Start the entire stack with:

```powershell
docker compose up --build
```

This starts:

- React frontend: `http://localhost:5173`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`
The frontend runs using the Vite development server and uses the Docker build arguments configured in `docker-compose.yml`.

## Keycloak

The Docker Compose setup imports `keycloak/user-directory-realm.json` automatically. It creates:

- Realm: `user-directory`
- API audience: `user-directory-api`
- React public client: `user-directory-frontend`
- Development user: `demo` / `demo123!`

The Keycloak admin console is available at `http://localhost:8082` with `admin` / `admin`.

- `Keycloak__Authority` - Keycloak realm issuer URL.
- `Keycloak__Audience` - expected JWT audience.
- `Keycloak__RequireHttpsMetadata` - set to `false` for local development.

All `/api/users` endpoints require a bearer token. The React client sends it as `Authorization: Bearer <token>`. CORS allows `http://localhost:5173` by default.

## API

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`

Create and update requests require a name of 2–100 characters, age from 0–120, required city and state, and a pincode of 4–10 characters.

## Docker

Start the API and frontend with:

```powershell
docker compose up --build
```

The API is exposed on `http://localhost:8080`; Keycloak is exposed on `http://localhost:8082`. The SQLite database is stored in the `user-directory-data` Docker volume.

## Tests

```powershell
dotnet test user-directory-dotnet.slnx
```

## Development disclosure

The implementation uses .NET 8, ASP.NET Core, EF Core SQLite, Swashbuckle, xUnit, Docker, and Keycloak JWT authentication. AI assistance was used during development through GitHub Copilot.
