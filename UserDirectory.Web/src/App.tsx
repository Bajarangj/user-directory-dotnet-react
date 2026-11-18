import { Navigate, Route, Routes } from 'react-router-dom';
import { Navigation } from './components/Navigation';
import { AddUserPage } from './pages/AddUserPage';
import { UserListPage } from './pages/UserListPage';

export function App() {
  return (
    <>
      <Navigation />
      <main className="container">
        <Routes>
          <Route path="/users" element={<UserListPage />} />
          <Route path="/users/add" element={<AddUserPage />} />
          <Route path="*" element={<Navigate to="/users" replace />} />
        </Routes>
      </main>
    </>
  );
}
