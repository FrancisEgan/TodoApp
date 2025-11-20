import type { LoginRequest, SignupRequest, SetPasswordRequest, LoginResponse } from '../types/auth';

const API_BASE_URL = 'https://localhost:7275';

export const authService = {
    async login(credentials: LoginRequest): Promise<LoginResponse> {
        const response = await fetch(`${API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(credentials),
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error || 'Login failed');
        }

        const data = await response.json();
        return data;
    },

    async signup(userData: SignupRequest): Promise<{ message: string; userId: number }> {
        const response = await fetch(`${API_BASE_URL}/auth/signup`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData),
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error || 'Signup failed');
        }

        const data = await response.json();
        return data;
    },

    async setPassword(passwordData: SetPasswordRequest): Promise<LoginResponse> {
        const response = await fetch(`${API_BASE_URL}/auth/set-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(passwordData),
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error || 'Failed to set password');
        }

        const data = await response.json();
        return data;
    },

    saveToken(token: string): void {
        localStorage.setItem('auth_token', token);
    },

    getToken(): string | null {
        return localStorage.getItem('auth_token');
    },

    removeToken(): void {
        localStorage.removeItem('auth_token');
    },

    saveUser(user: LoginResponse): void {
        localStorage.setItem('user', JSON.stringify(user));
    },

    getUser(): LoginResponse | null {
        const user = localStorage.getItem('user');
        return user ? JSON.parse(user) : null;
    },

    removeUser(): void {
        localStorage.removeItem('user');
    },

    logout(): void {
        this.removeToken();
        this.removeUser();
    },
};
