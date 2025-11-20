import { useAuth } from '../hooks/useAuth';
import './TodoDashboard.css';

export function TodoDashboard() {
  const { user, logout } = useAuth();

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>Todo App</h1>
        <div className="user-info">
          <span>Welcome, {user?.firstName}!</span>
          <button onClick={logout} className="logout-button">
            Logout
          </button>
        </div>
      </header>
      <main className="dashboard-content">
        <h2>Your Todos</h2>
        <p>Todo list coming soon...</p>
      </main>
    </div>
  );
}
