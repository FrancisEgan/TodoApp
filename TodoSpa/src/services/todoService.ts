import type { Todo, CreateTodoRequest, UpdateTodoRequest } from '../types/todo';
import { authService } from './authService';

const API_BASE_URL = 'https://localhost:7275';

export const todoService = {
    async getAll(): Promise<Todo[]> {
        const token = authService.getToken();
        const response = await fetch(`${API_BASE_URL}/todos`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });

        if (!response.ok) {
            throw new Error('Failed to fetch todos');
        }

        return response.json();
    },

    async create(todo: CreateTodoRequest): Promise<Todo> {
        const token = authService.getToken();
        const response = await fetch(`${API_BASE_URL}/todos`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(todo),
        });

        if (!response.ok) {
            throw new Error('Failed to create todo');
        }

        return response.json();
    },

    async update(id: number, updates: UpdateTodoRequest): Promise<Todo> {
        const token = authService.getToken();
        const response = await fetch(`${API_BASE_URL}/todos/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(updates),
        });

        if (!response.ok) {
            throw new Error('Failed to update todo');
        }

        return response.json();
    },

    async delete(id: number): Promise<void> {
        const token = authService.getToken();
        const response = await fetch(`${API_BASE_URL}/todos/${id}`, {
            method: 'DELETE',
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });

        if (!response.ok) {
            throw new Error('Failed to delete todo');
        }
    },
};
