# User Directory: SQLite and Keycloak End-to-End Guide

This document explains how SQLite and Keycloak are configured, how they work together, and how to run the User Directory backend locally or with Docker.

The repository currently contains the .NET 8 backend. The React frontend will be developed separately.

## 1. Architecture overview

The backend uses the following layers:

- `UserDirectory.API` - HTTP controllers, Swagger, CORS, authentication, health checks, and application startup.
- `UserDirectory.Application` - DTOs, validation, use cases, and repository interfaces.
- `UserDirectory.Domain` - the `User` entity and domain behavior.
- `UserDirectory.Infrastructure` - EF Core, SQLite, repositories, and dependency registration.
- `UserDirectory.Tests` - unit and controller contract tests.

The API uses Keycloak for authentication and SQLite for application data.

There are two different types of users:

1. **Keycloak users** authenticate to the application.
2. **Directory users** are stored in SQLite and represent the records managed by the API.

For example, the Keycloak user `demo` can authenticate, while a directory user such as `Jane Doe` is stored in SQLite.

## 2. SQLite configuration

SQLite is configured through Entity Framework Core.

### Package

`UserDirectory.Infrastructure/UserDirectory.Infrastructure.csproj` references:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.19" />
```

### Connection strings

Development configuration in `UserDirectory.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Data Source=./data/app.db"
  }
}
```

Default/container configuration in `UserDirectory.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Data Source=/data/app.db"
  }
}
```

### Local database location

When running the API directly from Visual Studio or with `dotnet run`, the database is stored at:

```plaintext
./data/app.db
```

Usually this is under the API project directory:

```plaintext
UserDirectory.API/data/app.db
```

The Infrastructure registration creates the parent directory automatically when it does not exist.

### Docker database location

When running in Docker, the database is stored at:

```plaintext
/data/app.db
```

Docker Compose maps this directory to a persistent volume:

```yaml
volumes:
  - user-directory-data:/data
```

The database remains available when the API container is recreated.

### EF Core context

The context is located at:

```plaintext
UserDirectory.Infrastructure/Persistence/UserDirectoryDbContext.cs
```

It contains the `Users` DbSet and configures:

- `Id` as the primary key.
- `Name` as required, maximum 100 characters.
- `Age` as required.
- `City` as required.
- `State` as required.
- `Pincode` as required, maximum 10 characters.

### Repository

The repository is located at:

```plaintext
UserDirectory.Infrastructure/Repositories/UserRepository.cs
```

It supports:

- Reading all users.
- Reading one user by ID.
- Adding users.
- Removing users.
- Saving changes.

The Application layer depends on `IUserRepository`, not directly on EF Core or SQLite.

### Database initialization

At API startup, the application calls:

```csharp
await dbContext.Database.EnsureCreatedAsync();
```

This creates the SQLite file and schema when they do not already exist.

`EnsureCreated` is appropriate for this assignment and local development. A production system should normally use reviewed EF Core migrations instead.

### Resetting local data

Stop the application and delete:

```plaintext
UserDirectory.API/data/app.db
```

Start the API again and the database will be recreated.

To delete the Docker database and volume:

```powershell
docker compose down -v
```

Warning: this permanently deletes the stored SQLite data in Docker.

## 3. Keycloak configuration

Keycloak is the OAuth2/OIDC identity provider. It authenticates users and issues JWT access tokens. The API validates those tokens.

The development realm definition is stored in:

```plaintext
keycloak/user-directory-realm.json
```

The realm name is:

```plaintext
user-directory
```

### API client

The realm contains the API audience/client:

```plaintext
user-directory-api
```

The API expects access tokens intended for this audience.

### React frontend client

The realm contains a public browser client:

```plaintext
user-directory-frontend
```

It is configured for the separate React application:

```plaintext
Redirect URI: http://localhost:5173/*
Web origin:   http://localhost:5173
```

The client is public and does not have a client secret. The frontend should use Authorization Code Flow with PKCE.

### Audience mapper

The frontend client has an audience mapper that adds:

```plaintext
user-directory-api
```

to the access token audience claim. This allows the backend to verify that the token was intended for the User Directory API.

### Development Keycloak user

The imported realm creates this local development user:

```plaintext
Username: demo
Password: demo123!
```

These credentials must not be used in production.

### Keycloak admin user

The Docker development instance uses:

```plaintext
Username: admin
Password: admin
```

These credentials are also development-only.

## 4. API Keycloak settings

The settings are in `UserDirectory.API/appsettings.json`:

```json
{
  "Keycloak": {
	"Authority": "http://localhost:8082/realms/user-directory",
	"Audience": "user-directory-api",
	"ValidIssuer": "http://localhost:8082/realms/user-directory",
	"RequireHttpsMetadata": false
  }
}
```

### Authority

`Authority` is the Keycloak realm URL used by the API to discover OpenID Connect metadata and signing keys.

When the API runs directly on the host:

```plaintext
http://localhost:8082/realms/user-directory
```

When the API runs inside Docker, Compose overrides this with the internal service address:

```plaintext
http://keycloak:8080/realms/user-directory
```

### Audience

The expected JWT audience is:

```plaintext
user-directory-api
```

Tokens without this audience are rejected.

### Valid issuer

The browser receives tokens with the externally accessible issuer:

```plaintext
http://localhost:8082/realms/user-directory
```

When the API runs in Docker, it discovers Keycloak internally through `keycloak:8080`, but validates the external issuer through `ValidIssuer`.

### HTTPS metadata

`RequireHttpsMetadata` is set to `false` for local development because Keycloak runs over HTTP.

Production should use HTTPS and set it to `true`.

## 5. Authentication middleware

The API configures JWT bearer authentication in `Program.cs`:

```csharp
builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(...);
```

The middleware pipeline contains:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Authentication must run before authorization.

`UsersController` has:

```csharp
[Authorize]
```

Therefore, all user endpoints require a valid access token:

```plaintext
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
```

Requests without a token or with an invalid/expired token return:

```plaintext
401 Unauthorized
```

## 6. Docker Compose

The Compose file is:

```plaintext
docker-compose.yml
```

It starts two services:

```plaintext
api
keycloak
```

### API container

The API container exposes:

```plaintext
http://localhost:8080
```

Important URLs:

```plaintext
Swagger: http://localhost:8080/swagger
Health:  http://localhost:8080/health
```

The API stores SQLite at `/data/app.db` and persists it through the `user-directory-data` volume.

The API connects to Keycloak internally using:

```plaintext
http://keycloak:8080/realms/user-directory
```

### Keycloak container

Keycloak is available from the host at:

```plaintext
http://localhost:8082
```

The realm import directory is mounted from:

```plaintext
./keycloak
```

to:

```plaintext
/opt/keycloak/data/import
```

Keycloak starts with:

```plaintext
start-dev --import-realm
```

This imports `user-directory-realm.json` automatically.

## 7. Run everything with Docker

From the repository root, run:

```powershell
docker compose up --build
```

The single root Dockerfile builds the API and React frontend. The services will be available at:

```plaintext
React:    http://localhost:5173
API:      http://localhost:8080
Swagger:  http://localhost:8080/swagger
Health:   http://localhost:8080/health
Keycloak: http://localhost:8082
```

Open the Keycloak admin console:

```plaintext
http://localhost:8082
```

Use:

```plaintext
Username: admin
Password: admin
```

The imported realm should be available as:

```plaintext
user-directory
```

## 8. Run the API locally without Docker

To run only Keycloak in Docker:

```powershell
docker compose up keycloak
```

Then run the API locally:

```powershell
dotnet run --project UserDirectory.API/UserDirectory.API.csproj
```

The API uses:

```plaintext
SQLite:  ./data/app.db
Keycloak: http://localhost:8082
```

Local API URLs:

```plaintext
HTTP Swagger:  http://localhost:5296/swagger
HTTPS Swagger: https://localhost:7064/swagger
Health:        http://localhost:5296/health
```

## 9. Authentication request flow

### Step 1: React redirects to Keycloak

The frontend uses this authority:

```plaintext
http://localhost:8082/realms/user-directory
```

The client ID is:

```plaintext
user-directory-frontend
```

The browser opens the Keycloak login page. The development user can log in with:

```plaintext
Username: demo
Password: demo123!
```

Keycloak redirects back to:

```plaintext
http://localhost:5173
```

The React application uses the authorization code and PKCE to obtain an access token.

### Step 2: React calls the API

The frontend sends the access token in the request header:

```http
GET http://localhost:8080/api/users
Authorization: Bearer eyJ...
```

### Step 3: API validates the token

The API verifies:

- Token signature.
- Token expiration.
- Token issuer.
- Token audience.
- JWT structure.

### Step 4: API accesses SQLite

The controller calls the Application service. The Application service calls `IUserRepository`, and the Infrastructure repository reads or writes SQLite through EF Core.

### Step 5: API returns JSON

Example response:

```json
[
  {
	"id": "d8f6f0b4-7a82-4d3e-83c0-f6f17e9a1f4c",
	"name": "Jane Doe",
	"age": 30,
	"city": "Seattle",
	"state": "WA",
	"pincode": "98101"
  }
]
```

## 10. API endpoints

All endpoints below require a bearer token.

```plaintext
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
```

Create and update requests require:

- Name: required, 2–100 characters.
- Age: required, 0–120.
- City: required.
- State: required.
- Pincode: required, 4–10 characters.

## 11. Testing with Swagger

Swagger is available at:

```plaintext
http://localhost:8080/swagger
```

To test authenticated endpoints:

1. Obtain an access token through the React frontend or another OIDC client.
2. Open Swagger.
3. Click **Authorize**.
4. Enter:

```plaintext
Bearer YOUR_ACCESS_TOKEN
```

5. Execute the API operation.

Without a valid token, Swagger returns `401 Unauthorized` for user endpoints.

## 12. Running tests

Run all tests with:

```powershell
dotnet test user-directory-dotnet.slnx
```

The current test suite covers:

- User creation.
- Validation errors.
- User updates.
- Missing-user deletion.
- Controller list responses.
- Controller not-found responses.
- Controller creation responses.
- Controller validation responses.

## 13. Production considerations

The current configuration is intended for local development and the coding assignment. Before production:

- Replace the Keycloak admin credentials.
- Replace the demo user password.
- Use HTTPS for Keycloak and the API.
- Set `RequireHttpsMetadata` to `true`.
- Store secrets in a secret manager or environment configuration.
- Replace `EnsureCreated` with reviewed EF Core migrations.
- Configure production CORS origins.
- Use production issuer and audience values.
- Configure persistent Keycloak storage.
- Do not use the development Docker settings in production.

The React frontend is not included in this repository. It should use:

```plaintext
Keycloak authority: http://localhost:8082/realms/user-directory
Client ID:           user-directory-frontend
API URL:             http://localhost:8080
```
