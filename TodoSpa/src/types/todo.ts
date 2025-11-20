export interface Todo {
    id: number;
    title: string;
    isComplete: boolean;
    createdBy: number;
    modifiedBy?: number;
    createdAt: string;
    modifiedAt?: string;
    isDeleted: boolean;
}

export interface CreateTodoRequest {
    title: string;
}

export interface UpdateTodoRequest {
    title?: string;
    isComplete?: boolean;
}
