import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import FormEditor from '../FormEditor';

describe('FormEditor', () => {
  const defaultProps = {
    id: 'test-editor',
    label: 'Test Editor Label',
    value: 'Test content',
    onChange: jest.fn(),
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('sets aria-label attribute to the editor element when label is provided', async () => {
    render(<FormEditor {...defaultProps} />);

    const editor = screen.getByRole('textbox');
    await waitFor(() =>
      expect(editor).toHaveAttribute('aria-label', 'Test Editor Label'),
    );
  });

  test('sets aria-label attribute when label changes', () => {
    const { rerender } = render(<FormEditor {...defaultProps} />);

    let editor = screen.getByRole('textbox');
    expect(editor).toHaveAttribute('aria-label', 'Test Editor Label');

    rerender(<FormEditor {...defaultProps} label="Updated Label" />);

    editor = screen.getByRole('textbox');
    expect(editor).toHaveAttribute('aria-label', 'Updated Label');
  });

  test('does not set aria-label when label is empty', () => {
    render(<FormEditor {...defaultProps} label="" />);

    const editor = screen.getByRole('textbox');
    expect(editor).not.toHaveAttribute('aria-label');
  });

  test('sets id attribute on editor element', () => {
    render(<FormEditor {...defaultProps} id="custom-editor-id" />);

    const editor = screen.getByRole('textbox');
    expect(editor).toHaveAttribute('id', 'custom-editor-id');
  });

  test('sets aria-describedby when error is provided', () => {
    render(<FormEditor {...defaultProps} error="Test error" />);

    const editor = screen.getByRole('textbox');
    expect(editor).toHaveAttribute('aria-describedby', 'test-editor-error');
  });

  test('sets aria-describedby when hint is provided', () => {
    render(<FormEditor {...defaultProps} hint="Test hint" />);

    const editor = screen.getByRole('textbox');
    expect(editor).toHaveAttribute('aria-describedby', 'test-editor-hint');
  });

  test('sets aria-describedby with both error and hint', () => {
    render(
      <FormEditor {...defaultProps} error="Test error" hint="Test hint" />,
    );

    const editor = screen.getByRole('textbox');
    expect(editor).toHaveAttribute(
      'aria-describedby',
      'test-editor-error test-editor-hint',
    );
  });

  test('textarea calls onChange when value changes', () => {
    const mockOnChange = jest.fn();
    render(<FormEditor {...defaultProps} onChange={mockOnChange} />);

    const textarea = screen.getByRole('textbox');
    fireEvent.change(textarea, { target: { value: 'new content' } });

    expect(mockOnChange).toHaveBeenCalledWith('new content');
  });

  test('textarea calls onBlur when losing focus', () => {
    const mockOnBlur = jest.fn();
    render(<FormEditor {...defaultProps} onBlur={mockOnBlur} />);

    const textarea = screen.getByRole('textbox');
    fireEvent.blur(textarea);

    expect(mockOnBlur).toHaveBeenCalled();
  });
});
