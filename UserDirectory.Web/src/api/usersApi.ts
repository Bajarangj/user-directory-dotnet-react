import type { User } from '../types/User';

const apiUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:8080';

export interface UserInput {
  name: string;
  age: number;
  city: string;
  state: string;
  pincode: string;
}

export class ApiError extends Error {
  constructor(message: string, public readonly status: number) {
    super(message);
    this.name = 'ApiError';
  }
}

async function request<T>(path: string, getToken: () => Promise<string>, options?: RequestInit): Promise<T> {
  const token = await getToken();
  if (!token) {
    throw new Error('An access token is required to call the API.');
  }

  const response = await fetch(`${apiUrl}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      ...options?.headers,
    },
  });

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`;
    try {
      const problem = await response.json() as { title?: string; detail?: string };
      message = problem.detail ?? problem.title ?? message;
    } catch {
      // Keep the status-based message when the response is not JSON.
    }

    throw new ApiError(message, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export function getUsers(getToken: () => Promise<string>) {
  return request<User[]>('/api/users', getToken);
}

export function createUser(getToken: () => Promise<string>, input: UserInput) {
  return request<User>('/api/users', getToken, {
    method: 'POST',
    body: JSON.stringify(input),
  });
}
