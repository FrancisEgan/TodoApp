import { useState, useRef, useEffect } from 'react';
import type { Todo } from '../types/todo';
import './TodoItem.scss';

interface TodoItemProps {
  todo: Todo;
  onUpdate: (id: number, title: string, isComplete: boolean) => Promise<void>;
  onDelete: (id: number) => Promise<void>;
}

export function TodoItem({ todo, onUpdate, onDelete }: TodoItemProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [title, setTitle] = useState(todo.title);
  const [isComplete, setIsComplete] = useState(todo.isComplete);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isEditing && inputRef.current) {
      inputRef.current.focus();
      inputRef.current.select();
    }
  }, [isEditing]);

  const handleSave = async () => {
    if (title.trim() === '') {
      setTitle(todo.title);
      setIsEditing(false);
      return;
    }

    if (title !== todo.title) {
      await onUpdate(todo.id, title, isComplete);
    }
    setIsEditing(false);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSave();
    } else if (e.key === 'Escape') {
      setTitle(todo.title);
      setIsEditing(false);
    }
  };

  const handleCheckboxChange = async (checked: boolean) => {
    setIsComplete(checked);
    await onUpdate(todo.id, title, checked);
  };

  return (
    <div className={`todo-item ${isComplete ? 'completed' : ''}`}>
      <input
        type="checkbox"
        checked={isComplete}
        onChange={(e) => handleCheckboxChange(e.target.checked)}
        className="todo-checkbox"
      />
      {isEditing ? (
        <input
          ref={inputRef}
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          onBlur={handleSave}
          onKeyDown={handleKeyDown}
          className="todo-input-edit"
        />
      ) : (
        <span
          className="todo-title"
          onClick={() => setIsEditing(true)}
          onDoubleClick={() => setIsEditing(true)}
        >
          {title}
        </span>
      )}
      <button
        onClick={() => onDelete(todo.id)}
        className="todo-delete"
        title="Delete todo"
      >
        ×
      </button>
    </div>
  );
}
