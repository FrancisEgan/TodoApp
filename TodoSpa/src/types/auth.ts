export interface LoginRequest {
    email: string;
    password: string;
}

export interface SignupRequest {
    email: string;
}

export interface VerifyAccountRequest {
    token: string;
    firstName: string;
    lastName: string;
    password: string;
}

export interface SetPasswordRequest {
    token: string;
    password: string;
}

export interface SignupResponse {
    message: string;
    userId: number;
}

export interface LoginResponse {
    token: string;
    userId: number;
    email: string;
    firstName: string;
    lastName: string;
}

export interface User {
    userId: number;
    email: string;
    firstName: string;
    lastName: string;
}
