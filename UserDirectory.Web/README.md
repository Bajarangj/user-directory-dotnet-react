# User Directory Web

React client for the User Directory .NET 8 API.

## Requirements

- Node.js 20 or newer
- npm
- The backend running from the repository root

## Start the complete stack

From the repository root:

```powershell
docker compose up --build
```

This provides:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`

- React frontend: `http://localhost:5173`

The development realm contains the public client `user-directory-frontend`, configured for `http://localhost:5173/*`.

## Run the frontend separately (optional)

From this directory:

```powershell
npm install
npm run dev
```

Open `http://localhost:5173`.

The normal Docker workflow does not require these commands because the frontend is built and served by the `web` Compose service.

The first visit redirects to Keycloak. Use `demo` / `demo123!` from the imported realm.

## Configuration

Copy `.env.example` to `.env.local` when overriding defaults:

```plaintext
VITE_API_URL=http://localhost:8080
VITE_KEYCLOAK_URL=http://localhost:8082
VITE_KEYCLOAK_REALM=user-directory
VITE_KEYCLOAK_CLIENT_ID=user-directory-frontend
```

The browser client uses Authorization Code Flow with PKCE and never contains a client secret.

## Application pages

- `/users` - authenticated user list with loading, empty, error, and success states.
- `/users/add` - validated form that creates a user and returns to the list.

## Build

```powershell
npm run build
npm run preview
```
