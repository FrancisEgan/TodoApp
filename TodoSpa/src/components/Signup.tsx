import { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import './Auth.css';

interface SignupProps {
  onSwitchToLogin: () => void;
  onSignupSuccess: (email: string) => void;
}

export function Signup({ onSwitchToLogin, onSignupSuccess }: SignupProps) {
  const { signup } = useAuth();
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccessMessage('');
    setIsLoading(true);

    try {
      const response = await signup({ firstName, lastName, email });
      setSuccessMessage(response.message + ' Check the API console for the verification token.');
      // In a real app, user would click link in email
      // For development, they'll need to get the token from API console
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Signup failed');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-form">
      <h2>Sign Up</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="firstName">First Name</label>
          <input
            id="firstName"
            type="text"
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            required
            disabled={isLoading}
          />
        </div>
        <div className="form-group">
          <label htmlFor="lastName">Last Name</label>
          <input
            id="lastName"
            type="text"
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            required
            disabled={isLoading}
          />
        </div>
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
        {error && <div className="error-message">{error}</div>}
        {successMessage && (
          <div className="success-message">
            <p>{successMessage}</p>
            <p className="small-text">
              Copy the token from the API console and use it to set your password below.
            </p>
          </div>
        )}
        <button type="submit" disabled={isLoading}>
          {isLoading ? 'Creating account...' : 'Sign Up'}
        </button>
      </form>
      <p className="auth-switch">
        Already have an account?{' '}
        <button type="button" onClick={onSwitchToLogin} className="link-button">
          Login
        </button>
      </p>
      {successMessage && (
        <button
          type="button"
          onClick={() => onSignupSuccess(email)}
          className="continue-button"
        >
          Continue to Set Password
        </button>
      )}
    </div>
  );
}
