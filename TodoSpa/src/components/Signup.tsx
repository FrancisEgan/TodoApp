import { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import './Auth.css';

interface SignupProps {
  onSwitchToLogin: () => void;
}

export function Signup({ onSwitchToLogin }: SignupProps) {
  const { signup } = useAuth();
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage('');
    setIsLoading(true);

    try {
      const response = await signup({ email });
      setMessage(response.message);
      setEmail('');
    } catch (err) {
      // Even on error, show the same message for security
      setMessage('Please check your email inbox to verify your account.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-form">
      <h2>Sign Up</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            disabled={isLoading}
          />
        </div>
        {message && (
          <div className="success-message">
            <p>{message}</p>
            <p className="small-text">
              Check the API console for the verification link.
            </p>
          </div>
        )}
        <button type="submit" disabled={isLoading}>
          {isLoading ? 'Sending...' : 'Sign Up'}
        </button>
      </form>
      <p className="auth-switch">
        Already have an account?{' '}
        <button type="button" onClick={onSwitchToLogin} className="link-button">
          Login
        </button>
      </p>
    </div>
  );
}
