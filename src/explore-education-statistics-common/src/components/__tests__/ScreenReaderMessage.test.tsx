import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import { act, render, screen, waitFor } from '@testing-library/react';
import React from 'react';

describe('ScreenReaderMessage', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  test('sets the message after a timeout', async () => {
    const { rerender } = render(<ScreenReaderMessage message="" />);

    rerender(<ScreenReaderMessage message="I am a message" />);

    expect(screen.queryByText('I am a message')).not.toBeInTheDocument();

    act(() => {
      jest.advanceTimersByTime(200);
    });

    await waitFor(() => {
      expect(screen.getByText('I am a message')).toBeInTheDocument();
    });
  });

  test('clears and then replaces the message', async () => {
    const { rerender } = render(<ScreenReaderMessage message="" />);

    rerender(<ScreenReaderMessage message="I am a message" />);

    expect(screen.queryByText('I am a message')).not.toBeInTheDocument();

    act(() => {
      jest.advanceTimersByTime(200);
    });

    await waitFor(() => {
      expect(screen.getByText('I am a message')).toBeInTheDocument();
    });

    rerender(<ScreenReaderMessage message="I am another message" />);

    expect(screen.queryByText('I am a message')).not.toBeInTheDocument();

    expect(screen.queryByText('I am another message')).not.toBeInTheDocument();

    act(() => {
      jest.advanceTimersByTime(200);
    });

    await waitFor(() => {
      expect(screen.getByText('I am another message')).toBeInTheDocument();
    });
  });
});
