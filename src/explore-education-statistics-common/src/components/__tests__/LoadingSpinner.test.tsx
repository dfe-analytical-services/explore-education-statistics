import LoadingSpinner from '@common/components/LoadingSpinner';
import { act, render, screen } from '@testing-library/react';
import React from 'react';

describe('LoadingSpinner', () => {
  test('does not render if `loading` is false', () => {
    render(<LoadingSpinner loading={false} />);

    expect(screen.queryByTestId('loadingSpinner')).not.toBeInTheDocument();
  });

  test('renders alert text with a slight delay for screen readers', async () => {
    jest.useFakeTimers();

    render(<LoadingSpinner alert text="Test text" />);

    expect(screen.queryByText('Test text')).not.toBeInTheDocument();

    await act(async () => {
      jest.runOnlyPendingTimers();
    });

    expect(await screen.findByText('Test text')).toBeInTheDocument();

    jest.useRealTimers();
  });

  test('re-renders new alert text with slight delay', async () => {
    jest.useFakeTimers();

    const { rerender } = render(<LoadingSpinner alert text="Test text" />);

    await act(async () => {
      jest.runOnlyPendingTimers();
    });

    expect(await screen.findByText('Test text')).toBeInTheDocument();

    rerender(<LoadingSpinner alert text="Test text updated" />);

    expect(screen.getByText('Test text')).toBeInTheDocument();

    await act(async () => {
      jest.runOnlyPendingTimers();
    });

    expect(await screen.findByText('Test text updated')).toBeInTheDocument();

    jest.useRealTimers();
  });

  test('renders non-alert text immediately', () => {
    render(<LoadingSpinner text="Test text" />);

    expect(screen.getByText('Test text')).toBeInTheDocument();
  });
});
