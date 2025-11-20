import { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { useTodos } from '../hooks/useTodos';
import { TodoItem } from '../components/TodoItem';
import { AddTodo } from '../components/AddTodo';
import './TodoDashboard.scss';

export function TodoDashboard() {
  const { user, logout } = useAuth();
  const { todos, isLoading, error, createTodo, updateTodo, deleteTodo } = useTodos();
  const [showAddForm, setShowAddForm] = useState(false);

  const handleAddTodo = async (title: string) => {
    await createTodo({ title });
    setShowAddForm(false);
  };

  const handleUpdateTodo = async (id: number, title: string, isComplete: boolean) => {
    await updateTodo(id, { title, isComplete });
  };

  const handleDeleteTodo = async (id: number) => {
    await deleteTodo(id);
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

        {error && <div className="error-message">Failed to load todos</div>}

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
