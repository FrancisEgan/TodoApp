import { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import './Auth.css';

interface SetPasswordProps {
  email: string;
  onBack: () => void;
}

export function SetPassword({ email, onBack }: SetPasswordProps) {
  const { setPassword } = useAuth();
  const [token, setToken] = useState('');
  const [password, setPasswordValue] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (password.length < 8) {
      setError('Password must be at least 8 characters');
      return;
    }

    if (!token) {
      setError('Please enter the verification token');
      return;
    }

    setIsLoading(true);

    try {
      await setPassword({ token, password });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to set password');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-form">
      <h2>Set Your Password</h2>
      <p className="info-message">Account created for {email}</p>
      <p className="info-message">Check the API console for your verification token</p>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="token">Verification Token</label>
          <input
            id="token"
            type="text"
            value={token}
            onChange={(e) => setToken(e.target.value)}
            required
            disabled={isLoading}
            placeholder="Paste token from console"
          />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPasswordValue(e.target.value)}
            required
            disabled={isLoading}
            minLength={8}
          />
        </div>
        <div className="form-group">
          <label htmlFor="confirmPassword">Confirm Password</label>
          <input
            id="confirmPassword"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            disabled={isLoading}
            minLength={8}
          />
        </div>
        {error && <div className="error-message">{error}</div>}
        <button type="submit" disabled={isLoading}>
          {isLoading ? 'Setting password...' : 'Set Password & Login'}
        </button>
      </form>
      <button type="button" onClick={onBack} className="link-button">
        Back to signup
      </button>
    </div>
  );
}
