import { useState, useRef, useEffect } from 'react';
import './AddTodo.scss';

interface AddTodoProps {
  onAdd: (title: string) => Promise<void>;
  onCancel?: () => void;
  autoFocus?: boolean;
}

export function AddTodo({ onAdd, onCancel, autoFocus = false }: AddTodoProps) {
  const [title, setTitle] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (autoFocus && inputRef.current) {
      inputRef.current.focus();
    }
  }, [autoFocus]);

  const handleSave = async () => {
    if (title.trim() === '' || isLoading) {
      if (onCancel) onCancel();
      return;
    }

    setIsLoading(true);
    try {
      await onAdd(title.trim());
      setTitle('');
    } catch (error) {
      console.error('Failed to add todo:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSave();
    } else if (e.key === 'Escape' && onCancel) {
      onCancel();
    }
  };

  const handleBlur = () => {
    handleSave();
  };

  return (
    <div className="add-todo-form">
      <input
        ref={inputRef}
        type="text"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        onKeyDown={handleKeyDown}
        onBlur={handleBlur}
        placeholder="What needs to be done?"
        className="add-todo-input"
        disabled={isLoading}
      />
    </div>
  );
}
