import { NavLink } from 'react-router-dom';
import { useAuth } from '../auth/AuthProvider';

export function Navigation() {
  const { username, logout } = useAuth();

  return (
    <header className="topbar">
      <NavLink className="brand" to="/users">User Directory</NavLink>
      <nav aria-label="Main navigation">
        <NavLink className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'} to="/users">List</NavLink>
        <NavLink className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'} to="/users/add">Add</NavLink>
      </nav>
      <div className="account">
        <span>{username ?? 'Signed in'}</span>
        <button className="button secondary small" type="button" onClick={logout}>Sign out</button>
      </div>
    </header>
  );
}
