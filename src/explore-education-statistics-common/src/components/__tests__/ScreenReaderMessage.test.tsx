import ScreenReaderMessage from '@common/components/ScreenReaderMessage';
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('ScreenReaderMessage', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });
  afterEach(() => {
    jest.useRealTimers();
    jest.runOnlyPendingTimers();
  });

  test('sets the message after a timeout', () => {
    const { rerender } = render(<ScreenReaderMessage message="" />);

    rerender(<ScreenReaderMessage message="I am a message" />);

    expect(screen.queryByText('I am a message')).not.toBeInTheDocument();

    jest.advanceTimersByTime(200);

    expect(screen.getByText('I am a message')).toBeInTheDocument();
  });

  test('clears and then replaces the message', () => {
    const { rerender } = render(<ScreenReaderMessage message="" />);

    rerender(<ScreenReaderMessage message="I am a message" />);

    expect(screen.queryByText('I am a message')).not.toBeInTheDocument();

    jest.advanceTimersByTime(200);

    expect(screen.getByText('I am a message')).toBeInTheDocument();

    rerender(<ScreenReaderMessage message="I am another message" />);

    expect(screen.queryByText('I am a message')).not.toBeInTheDocument();

    expect(screen.queryByText('I am another message')).not.toBeInTheDocument();

    jest.advanceTimersByTime(200);

    expect(screen.getByText('I am another message')).toBeInTheDocument();
  });
});
