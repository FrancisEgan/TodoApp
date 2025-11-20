import { createContext } from 'react';
import type { User, LoginRequest, SignupRequest, VerifyAccountRequest } from '../types/auth';

export interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  signup: (userData: SignupRequest) => Promise<{ message: string }>;
  verifyAccount: (verifyData: VerifyAccountRequest) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
