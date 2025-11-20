import { useState } from 'react';
import { Login } from '../components/Login';
import { Signup } from '../components/Signup';
import './LandingPage.css';

type View = 'login' | 'signup';

export function LandingPage() {
  const [currentView, setCurrentView] = useState<View>('login');

  return (
    <div className="landing-page">
      <div className="landing-content">
        <div className="landing-header">
          <h1>Todo App</h1>
          <p>Organize your tasks and boost your productivity</p>
        </div>

        {currentView === 'login' && (
          <Login onSwitchToSignup={() => setCurrentView('signup')} />
        )}

        {currentView === 'signup' && (
          <Signup onSwitchToLogin={() => setCurrentView('login')} />
        )}
      </div>
    </div>
  );
}
