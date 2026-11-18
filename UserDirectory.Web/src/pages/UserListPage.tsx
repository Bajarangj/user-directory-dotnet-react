import { useEffect, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { getUsers } from '../api/usersApi';
import { useAuth } from '../auth/AuthProvider';
import type { User } from '../types/User';

export function UserListPage() {
  const { getToken } = useAuth();
  const location = useLocation();
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string>();

  useEffect(() => {
    let active = true;

    getUsers(getToken)
      .then(result => {
        if (active) setUsers(result);
      })
      .catch(exception => {
        if (active) setError(exception instanceof Error ? exception.message : 'Unable to load users.');
      })
      .finally(() => {
        if (active) setIsLoading(false);
      });

    return () => {
      active = false;
    };
  }, [getToken]);

  const successMessage = (location.state as { success?: string } | null)?.success;

  return (
    <section className="page-card">
      <div className="page-heading">
        <div>
          <p className="eyebrow">Directory</p>
          <h1>Users</h1>
        </div>
        <Link className="button primary" to="/users/add">Add user</Link>
      </div>

      {successMessage && <div className="toast success" role="status">{successMessage}</div>}
      {isLoading && <div className="screen-state compact">Loading users...</div>}
      {error && <div className="alert error" role="alert">{error}</div>}

      {!isLoading && !error && users.length === 0 && (
        <div className="empty-state">
          <h2>No users yet</h2>
          <p>Add the first user to start building the directory.</p>
          <Link className="button primary" to="/users/add">Add first user</Link>
        </div>
      )}

      {!isLoading && !error && users.length > 0 && (
        <div className="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Age</th>
                <th>City</th>
                <th>State</th>
                <th>Pincode</th>
              </tr>
            </thead>
            <tbody>
              {users.map(user => (
                <tr key={user.id}>
                  <td>{user.name}</td>
                  <td>{user.age}</td>
                  <td>{user.city}</td>
                  <td>{user.state}</td>
                  <td>{user.pincode}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
