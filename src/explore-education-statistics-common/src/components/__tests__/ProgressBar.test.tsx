import ProgressBar from '@common/components/ProgressBar';
import { render, screen } from '@testing-library/react';
import React from 'react';

describe('ProgressBar', () => {
  test('renders correctly at 0%', () => {
    render(<ProgressBar value={0} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '0',
    );

    expect(screen.getByText('0% complete')).toBeInTheDocument();
  });

  test('renders correctly at 7%', () => {
    render(<ProgressBar value={7} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '7',
    );
    expect(screen.getByText('7% complete')).toBeInTheDocument();
  });

  test('renders correctly at 50%', () => {
    render(<ProgressBar value={50} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '50',
    );
    expect(screen.getByText('50% complete')).toBeInTheDocument();
  });

  test('renders correctly at 100%', () => {
    render(<ProgressBar value={100} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '100',
    );
    expect(screen.getByText('100% complete')).toBeInTheDocument();
  });

  test('renders 200% clamped at 100%', () => {
    render(<ProgressBar value={200} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '100',
    );

    expect(screen.getByText('100% complete')).toBeInTheDocument();
  });

  test('renders -100% clamped at 0%', () => {
    render(<ProgressBar value={-100} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '0',
    );

    expect(screen.getByText('0% complete')).toBeInTheDocument();
  });

  test('renders correctly with custom `min` prop', () => {
    render(<ProgressBar value={75} min={50} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuemin',
      '50',
    );
    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuemax',
      '100',
    );
    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '75',
    );
    expect(screen.getByText('50% complete')).toBeInTheDocument();
  });

  test('renders correctly with custom `max` prop', () => {
    render(<ProgressBar value={40} max={50} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuemin',
      '0',
    );
    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuemax',
      '50',
    );
    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '40',
    );
    expect(screen.getByText('80% complete')).toBeInTheDocument();
  });

  test('renders correctly with custom `min` and `max` prop', () => {
    render(<ProgressBar value={26} min={20} max={30} />);

    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuemin',
      '20',
    );
    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuemax',
      '30',
    );
    expect(screen.getByRole('progressbar')).toHaveAttribute(
      'aria-valuenow',
      '26',
    );
    expect(screen.getByText('60% complete')).toBeInTheDocument();
  });
});
