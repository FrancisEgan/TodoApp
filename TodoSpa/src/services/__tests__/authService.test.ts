import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from '../../services/authService';
import type { LoginRequest, SignupRequest, VerifyAccountRequest } from '../../types/auth';

// Mock fetch globally
const mockFetch = vi.fn();
globalThis.fetch = mockFetch as typeof fetch;

describe('authService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('login', () => {
    it('successfully logs in and returns user data', async () => {
      const mockResponse = {
        token: 'test-token',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'password123',
      };

      const result = await authService.login(credentials);

      expect(result).toEqual(mockResponse);
      expect(mockFetch).toHaveBeenCalledWith(
        'https://localhost:7275/auth/login',
        expect.objectContaining({
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(credentials),
        })
      );
    });

    it('throws error on failed login', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        text: async () => 'Invalid credentials',
      });

      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'wrong-password',
      };

      await expect(authService.login(credentials)).rejects.toThrow('Invalid credentials');
    });
  });

  describe('signup', () => {
    it('successfully signs up', async () => {
      const mockResponse = { message: 'Account created successfully' };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const userData: SignupRequest = {
        email: 'test@example.com',
      };

      const result = await authService.signup(userData);

      expect(result).toEqual(mockResponse);
    });

    it('throws error on failed signup', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        text: async () => 'Email already exists',
      });

      const userData: SignupRequest = {
        email: 'existing@example.com',
      };

      await expect(authService.signup(userData)).rejects.toThrow('Email already exists');
    });
  });

  describe('verifyAccount', () => {
    it('successfully verifies account', async () => {
      const mockResponse = {
        token: 'test-token',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const verifyData: VerifyAccountRequest = {
        token: '123456',
        firstName: 'John',
        lastName: 'Doe',
        password: 'password123',
      };

      const result = await authService.verifyAccount(verifyData);

      expect(result).toEqual(mockResponse);
    });
  });

  describe('token management', () => {
    it('saves and retrieves token', () => {
      authService.saveToken('test-token');
      expect(authService.getToken()).toBe('test-token');
    });

    it('removes token', () => {
      authService.saveToken('test-token');
      authService.removeToken();
      expect(authService.getToken()).toBeNull();
    });
  });

  describe('user management', () => {
    it('saves and retrieves user', () => {
      const user = {
        token: 'test-token',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
      };

      authService.saveUser(user);
      expect(authService.getUser()).toEqual(user);
    });

    it('removes user', () => {
      const user = {
        token: 'test-token',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
      };

      authService.saveUser(user);
      authService.removeUser();
      expect(authService.getUser()).toBeNull();
    });
  });

  describe('logout', () => {
    it('clears token and user data', () => {
      const user = {
        token: 'test-token',
        userId: 1,
        email: 'test@example.com',
        firstName: 'John',
        lastName: 'Doe',
      };

      authService.saveToken('test-token');
      authService.saveUser(user);

      authService.logout();

      expect(authService.getToken()).toBeNull();
      expect(authService.getUser()).toBeNull();
    });
  });
});
