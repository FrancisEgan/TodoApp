import { useSearchParams, Navigate } from 'react-router-dom';
import { VerifyAccount } from '../components/VerifyAccount';
import './VerifyPage.scss';

export function VerifyPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  if (!token) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="verify-page">
      <div className="verify-content">
        <div className="verify-header">
          <h1>Todo App</h1>
          <p>Complete your registration</p>
        </div>
        <VerifyAccount token={token} />
      </div>
    </div>
  );
}
