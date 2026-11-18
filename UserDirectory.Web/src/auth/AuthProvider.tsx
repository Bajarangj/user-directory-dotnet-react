import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import keycloak, { initializeKeycloak } from './keycloak';

interface AuthContextValue {
  isAuthenticated: boolean;
  isLoading: boolean;
  username?: string;
  logout: () => void;
  getToken: () => Promise<string>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let active = true;

    initializeKeycloak()
      .then(authenticated => {
        if (active) {
          setIsAuthenticated(authenticated);
        }
      })
      .catch(() => {
        if (active) {
          setIsAuthenticated(false);
        }
      })
      .finally(() => {
        if (active) {
          setIsLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, []);

  const getToken = async () => {
    if (!keycloak.authenticated) {
      throw new Error('You are not authenticated.');
    }

    await keycloak.updateToken(30);
    if (!keycloak.token) {
      throw new Error('Keycloak did not return an access token.');
    }

    return keycloak.token;
  };

  const value = useMemo<AuthContextValue>(() => ({
    isAuthenticated,
    isLoading,
    username: keycloak.tokenParsed?.preferred_username,
    logout: () => keycloak.logout({ redirectUri: window.location.origin }),
    getToken,
  }), [isAuthenticated, isLoading]);

  if (isLoading) {
    return <div className="screen-state">Connecting to sign-in...</div>;
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider.');
  }

  return context;
}
