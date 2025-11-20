import { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import { todoService } from '../services/todoService';
import { TodoItem } from '../components/TodoItem';
import { AddTodo } from '../components/AddTodo';
import type { Todo } from '../types/todo';
import './TodoDashboard.scss';

export function TodoDashboard() {
  const { user, logout } = useAuth();
  const [todos, setTodos] = useState<Todo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [showAddForm, setShowAddForm] = useState(false);

  useEffect(() => {
    loadTodos();
  }, []);

  const loadTodos = async () => {
    try {
      setIsLoading(true);
      const data = await todoService.getAll();
      setTodos(data);
      setError('');
    } catch (err) {
      setError('Failed to load todos');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddTodo = async (title: string) => {
    try {
      const newTodo = await todoService.create({ title });
      setTodos([...todos, newTodo]);
      setShowAddForm(false);
    } catch (err) {
      console.error('Failed to add todo:', err);
      throw err;
    }
  };

  const handleUpdateTodo = async (id: number, title: string, isComplete: boolean) => {
    try {
      const updated = await todoService.update(id, { title, isComplete });
      setTodos(todos.map((t) => (t.id === id ? updated : t)));
    } catch (err) {
      console.error('Failed to update todo:', err);
      // Revert on error
      await loadTodos();
    }
  };

  const handleDeleteTodo = async (id: number) => {
    try {
      await todoService.delete(id);
      setTodos(todos.filter((t) => t.id !== id));
    } catch (err) {
      console.error('Failed to delete todo:', err);
    }
  };

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>Todo App</h1>
        <div className="user-info">
          <span>Welcome, {user?.firstName}!</span>
          <button onClick={logout} className="logout-button">
            Logout
          </button>
        </div>
      </header>
      <main className="dashboard-content">
        <div className="todos-header">
          <h2>Your Todos</h2>
          <div className="todos-stats">
            {todos.length > 0 && (
              <span>
                {todos.filter((t) => t.isComplete).length} of {todos.length} completed
              </span>
            )}
          </div>
        </div>

        {error && <div className="error-message">{error}</div>}

        <div className="todos-container">
          {isLoading ? (
            <div className="loading">Loading todos...</div>
          ) : todos.length === 0 && !showAddForm ? (
            <div className="empty-state">
              <p>No todos yet. Start by adding your first task!</p>
              <button onClick={() => setShowAddForm(true)} className="add-first-todo-button">
                + Add Your First Todo
              </button>
            </div>
          ) : (
            <>
              <div className="todos-grid">
                {todos.map((todo) => (
                  <TodoItem
                    key={todo.id}
                    todo={todo}
                    onUpdate={handleUpdateTodo}
                    onDelete={handleDeleteTodo}
                  />
                ))}
              </div>
              {showAddForm ? (
                <AddTodo onAdd={handleAddTodo} onCancel={() => setShowAddForm(false)} autoFocus />
              ) : (
                <button onClick={() => setShowAddForm(true)} className="add-todo-trigger">
                  <span className="plus-icon">+</span>
                  <span>Add new todo</span>
                </button>
              )}
            </>
          )}
        </div>
      </main>
    </div>
  );
}
