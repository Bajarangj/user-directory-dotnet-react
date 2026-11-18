import Keycloak from 'keycloak-js';

const keycloak = new Keycloak({
  url: import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8082',
  realm: import.meta.env.VITE_KEYCLOAK_REALM ?? 'user-directory',
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'user-directory-frontend',
});

let initialization: Promise<boolean> | undefined;

export function initializeKeycloak() {
  initialization ??= keycloak.init({
    onLoad: 'login-required',
    pkceMethod: 'S256',
    checkLoginIframe: false,
  });

  return initialization;
}

export default keycloak;
