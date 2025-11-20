import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { TodoItem } from '../../components/TodoItem';
import type { Todo } from '../../types/todo';

describe('TodoItem', () => {
  const mockTodo: Todo = {
    id: 1,
    title: 'Test Todo',
    isComplete: false,
    createdAt: new Date().toISOString(),
  };

  const mockOnUpdate = vi.fn();
  const mockOnDelete = vi.fn();

  beforeEach(() => {
    mockOnUpdate.mockClear();
    mockOnDelete.mockClear();
  });

  it('renders todo item correctly', () => {
    render(
      <TodoItem todo={mockTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    expect(screen.getByText('Test Todo')).toBeInTheDocument();
    expect(screen.getByRole('checkbox')).not.toBeChecked();
  });

  it('toggles completion status when checkbox is clicked', async () => {
    render(
      <TodoItem todo={mockTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    const checkbox = screen.getByRole('checkbox');
    fireEvent.click(checkbox);

    await waitFor(() => {
      expect(mockOnUpdate).toHaveBeenCalledWith(1, 'Test Todo', true);
    });
  });

  it('enters edit mode when title is clicked', () => {
    render(
      <TodoItem todo={mockTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    const title = screen.getByText('Test Todo');
    fireEvent.click(title);

    expect(screen.getByDisplayValue('Test Todo')).toBeInTheDocument();
  });

  it('updates title when edited and blurred', async () => {
    render(
      <TodoItem todo={mockTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    const title = screen.getByText('Test Todo');
    fireEvent.click(title);

    const input = screen.getByDisplayValue('Test Todo');
    fireEvent.change(input, { target: { value: 'Updated Todo' } });
    fireEvent.blur(input);

    await waitFor(() => {
      expect(mockOnUpdate).toHaveBeenCalledWith(1, 'Updated Todo', false);
    });
  });

  it('saves on Enter key press', async () => {
    render(
      <TodoItem todo={mockTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    const title = screen.getByText('Test Todo');
    fireEvent.click(title);

    const input = screen.getByDisplayValue('Test Todo');
    fireEvent.change(input, { target: { value: 'Updated Todo' } });
    fireEvent.keyDown(input, { key: 'Enter' });

    await waitFor(() => {
      expect(mockOnUpdate).toHaveBeenCalledWith(1, 'Updated Todo', false);
    });
  });

  it('cancels edit on Escape key press', () => {
    render(
      <TodoItem todo={mockTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    const title = screen.getByText('Test Todo');
    fireEvent.click(title);

    const input = screen.getByDisplayValue('Test Todo');
    fireEvent.change(input, { target: { value: 'Updated Todo' } });
    fireEvent.keyDown(input, { key: 'Escape' });

    expect(screen.getByText('Test Todo')).toBeInTheDocument();
    expect(mockOnUpdate).not.toHaveBeenCalled();
  });

  it('reverts to original title if empty string is submitted', async () => {
    render(
      <TodoItem todo={mockTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    const title = screen.getByText('Test Todo');
    fireEvent.click(title);

    const input = screen.getByDisplayValue('Test Todo');
    fireEvent.change(input, { target: { value: '   ' } });
    fireEvent.blur(input);

    await waitFor(() => {
      expect(screen.getByText('Test Todo')).toBeInTheDocument();
    });
    expect(mockOnUpdate).not.toHaveBeenCalled();
  });

  it('calls onDelete when delete button is clicked', () => {
    render(
      <TodoItem todo={mockTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    const deleteButton = screen.getByTitle('Delete todo');
    fireEvent.click(deleteButton);

    expect(mockOnDelete).toHaveBeenCalledWith(1);
  });

  it('applies completed class when todo is complete', () => {
    const completedTodo: Todo = { ...mockTodo, isComplete: true };
    const { container } = render(
      <TodoItem todo={completedTodo} onUpdate={mockOnUpdate} onDelete={mockOnDelete} />
    );

    const todoItem = container.querySelector('.todo-item');
    expect(todoItem).toHaveClass('completed');
  });
});
