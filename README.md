# User Directory

Full-stack .NET 8 user directory with ASP.NET Core, React/Vite, SQLite, Docker Compose, and Keycloak.

## Docker quick start

From the repository root:

```powershell
docker compose down --remove-orphans
docker compose up --build
``` 

URLs: frontend http://localhost:5173; API http://localhost:8080; Swagger http://localhost:8080/swagger; health http://localhost:8080/health; Keycloak http://localhost:8082.

## Demo login

Keycloak realm: user-directory

```text
Application: demo / demo123!
Keycloak admin: admin / admin
``` 

Open http://localhost:5173 and sign in with demo / demo123!. These credentials are for local demo use only.

## Authentication

The React application uses keycloak-js with Authorization Code Flow and PKCE (S256). Frontend client: user-directory-frontend. API audience: user-directory-api. Requests send Authorization: Bearer <access-token>. The API validates JWT signature, issuer, audience, and expiration. Browser issuer: http://localhost:8082/realms/user-directory. Docker discovery: http://keycloak:8080/realms/user-directory.

## Architecture

- UserDirectory.API: controllers, Swagger, CORS, authentication, and composition.
- UserDirectory.Application: DTOs, validation, services, and repository abstractions.
- UserDirectory.Domain: User entity and domain model.
- UserDirectory.Infrastructure: EF Core SQLite DbContext and repositories.
- UserDirectory.Tests: xUnit tests.
- UserDirectory.Web: React pages, routing, Keycloak, and API client.
- keycloak/user-directory-realm.json: imported local realm.

## API

- GET /health - anonymous health check.
- GET /api/users - list users.
- GET /api/users/{id} - get a user.
- POST /api/users - create a user.
- PUT /api/users/{id} - update a user.
- DELETE /api/users/{id} - delete a user.

All user endpoints require authentication. Name is 2-100 characters, age is 0-120, city and state are required, and pincode is 4-10 characters.

## Technologies and packages

Backend: .NET 8, ASP.NET Core, EF Core SQLite 8.0.19, Microsoft.AspNetCore.Authentication.JwtBearer 8.0.19, Swashbuckle.AspNetCore 6.6.2.

Frontend: React 19.0.0, React DOM 19.0.0, React Router DOM 6.28.1, keycloak-js 26.2.2, Vite 6.0.7, TypeScript 5.7.2, @vitejs/plugin-react 4.3.4.

Tests: xUnit 2.9.2, Microsoft.NET.Test.Sdk 17.11.1, xunit.runner.visualstudio 2.8.2.

Infrastructure: Docker Compose, Keycloak 25.0, and SQLite.

## Persistence

SQLite is stored at /data/app.db and persisted in the user-directory-data Docker volume. Reset demo data with:

```powershell
docker compose down -v
docker compose up --build
``` 

## Configuration

Frontend defaults are in UserDirectory.Web/.env.example: VITE_API_URL, VITE_KEYCLOAK_URL, VITE_KEYCLOAK_REALM, and VITE_KEYCLOAK_CLIENT_ID.

API Docker settings: Keycloak__Authority=http://keycloak:8080/realms/user-directory; Keycloak__Issuer=http://localhost:8082/realms/user-directory; Keycloak__Audience=user-directory-api; Keycloak__RequireHttpsMetadata=false.

HTTP is for local development only. Production requires HTTPS and production secrets.

## Validation

```powershell
dotnet build user-directory-dotnet.slnx
dotnet test user-directory-dotnet.slnx
docker compose config
``` 

For frontend validation from UserDirectory.Web: npm install; npm run build.

## Troubleshooting

```powershell
docker compose down --remove-orphans
docker compose up --build --force-recreate
docker compose logs -f keycloak
``` 

For invalid_token, confirm iss is http://localhost:8082/realms/user-directory and aud is user-directory-api. For frontend 401 responses, confirm login completed and the request contains a bearer access token.

The root README.md is the authoritative documentation.