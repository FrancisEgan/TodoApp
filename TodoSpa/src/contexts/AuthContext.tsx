import { createContext, useState } from 'react';
import type { ReactNode } from 'react';
import type { User, LoginRequest, SignupRequest, VerifyAccountRequest } from '../types/auth';
import { authService } from '../services/authService';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  signup: (userData: SignupRequest) => Promise<{ message: string }>;
  verifyAccount: (verifyData: VerifyAccountRequest) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    // Initialize state from localStorage
    const savedUser = authService.getUser();
    const token = authService.getToken();

    if (savedUser && token) {
      return {
        userId: savedUser.userId,
        email: savedUser.email,
        firstName: savedUser.firstName,
        lastName: savedUser.lastName,
      };
    }
    return null;
  });
  const isLoading = false;
  const isAuthenticated = !!user;

  const login = async (credentials: LoginRequest) => {
    const response = await authService.login(credentials);
    console.log('Login response:', response);
    authService.saveToken(response.token);
    authService.saveUser(response);
    console.log('Token saved:', authService.getToken());
    console.log('User saved:', authService.getUser());
    setUser({
      userId: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
    });
  };

  const signup = async (userData: SignupRequest) => {
    return await authService.signup(userData);
  };

  const verifyAccount = async (verifyData: VerifyAccountRequest) => {
    const response = await authService.verifyAccount(verifyData);
    authService.saveToken(response.token);
    authService.saveUser(response);
    setUser({
      userId: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
    });
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated,
        isLoading,
        login,
        signup,
        verifyAccount,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
