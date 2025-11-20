import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { todoService } from '../services/todoService';
import type { Todo, CreateTodoRequest, UpdateTodoRequest } from '../types/todo';

const TODOS_QUERY_KEY = ['todos'];

export function useTodos() {
    const queryClient = useQueryClient();

    const todosQuery = useQuery({
        queryKey: TODOS_QUERY_KEY,
        queryFn: todoService.getAll,
    });

    const createMutation = useMutation({
        mutationFn: todoService.create,
        onMutate: async (newTodo: CreateTodoRequest) => {
            // Cancel outgoing refetches
            await queryClient.cancelQueries({ queryKey: TODOS_QUERY_KEY });

            // Snapshot previous value
            const previousTodos = queryClient.getQueryData<Todo[]>(TODOS_QUERY_KEY);

            // Optimistically update with temporary ID
            queryClient.setQueryData<Todo[]>(TODOS_QUERY_KEY, (old) => [
                ...(old || []),
                {
                    id: Date.now(), // Temporary ID
                    title: newTodo.title,
                    isComplete: false,
                    createdAt: new Date().toISOString(),
                },
            ]);

            return { previousTodos };
        },
        onError: (_err, _newTodo, context) => {
            // Rollback on error
            queryClient.setQueryData(TODOS_QUERY_KEY, context?.previousTodos);
        },
        onSuccess: () => {
            // Refetch to get the actual ID from the server
            queryClient.invalidateQueries({ queryKey: TODOS_QUERY_KEY });
        },
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, updates }: { id: number; updates: UpdateTodoRequest }) =>
            todoService.update(id, updates),
        onMutate: async ({ id, updates }) => {
            await queryClient.cancelQueries({ queryKey: TODOS_QUERY_KEY });

            const previousTodos = queryClient.getQueryData<Todo[]>(TODOS_QUERY_KEY);

            // Optimistically update
            queryClient.setQueryData<Todo[]>(TODOS_QUERY_KEY, (old) =>
                old?.map((todo) =>
                    todo.id === id
                        ? { ...todo, ...updates }
                        : todo
                ) || []
            );

            return { previousTodos };
        },
        onError: (_err, _variables, context) => {
            queryClient.setQueryData(TODOS_QUERY_KEY, context?.previousTodos);
        },
        onSuccess: () => {
            // Delay the refetch slightly to allow CSS transition to complete
            setTimeout(() => {
                queryClient.invalidateQueries({ queryKey: TODOS_QUERY_KEY });
            }, 300);
        },
    });

    const deleteMutation = useMutation({
        mutationFn: todoService.delete,
        onMutate: async (id: number) => {
            await queryClient.cancelQueries({ queryKey: TODOS_QUERY_KEY });

            const previousTodos = queryClient.getQueryData<Todo[]>(TODOS_QUERY_KEY);

            // Optimistically remove
            queryClient.setQueryData<Todo[]>(TODOS_QUERY_KEY, (old) =>
                old?.filter((todo) => todo.id !== id) || []
            );

            return { previousTodos };
        },
        onError: (_err, _id, context) => {
            queryClient.setQueryData(TODOS_QUERY_KEY, context?.previousTodos);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: TODOS_QUERY_KEY });
        },
    });

    return {
        todos: todosQuery.data || [],
        isLoading: todosQuery.isLoading,
        error: todosQuery.error,
        createTodo: createMutation.mutateAsync,
        updateTodo: (id: number, updates: UpdateTodoRequest) =>
            updateMutation.mutateAsync({ id, updates }),
        deleteTodo: deleteMutation.mutateAsync,
    };
}
