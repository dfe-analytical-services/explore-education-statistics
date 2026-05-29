import render from '@common-test/render';
import ExpandableFilterGroup from '@frontend/modules/search-data/components/ExpandableFilterGroup';
import { screen } from '@testing-library/react';
import React from 'react';

describe('ExpandableFilterGroup', () => {
  test('renders correctly with required props', () => {
    render(
      <ExpandableFilterGroup label="Test label" id="test-id">
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    expect(
      screen.getByRole('button', { name: 'Test label - show options' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Test content')).toBeInTheDocument();
  });

  test('renders with labelSub', () => {
    render(
      <ExpandableFilterGroup
        label="Test label"
        labelSub="Test sublabel"
        id="test-id"
      >
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    expect(screen.getByText('Test sublabel')).toBeInTheDocument();
  });

  test('renders with labelAfter content', () => {
    render(
      <ExpandableFilterGroup
        label="Test label"
        labelAfter={<span>After label content</span>}
        id="test-id"
      >
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    expect(screen.getByText('After label content')).toBeInTheDocument();
  });

  test('renders with custom labelHiddenText', () => {
    render(
      <ExpandableFilterGroup
        label="Test label"
        labelHiddenText=" - custom hidden text"
        id="test-id"
      >
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    expect(
      screen.getByRole('button', { name: 'Test label - custom hidden text' }),
    ).toBeInTheDocument();
  });

  test('renders with custom testId', () => {
    render(
      <ExpandableFilterGroup
        label="Test label"
        id="test-id"
        testId="custom-test-id"
      >
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    expect(screen.getByTestId('custom-test-id')).toBeInTheDocument();
  });

  test('is closed by default', () => {
    render(
      <ExpandableFilterGroup label="Test label" id="test-id">
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - show options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'false');
    expect(button).toHaveAttribute('aria-controls', 'test-id-content');
  });

  test('is expanded when open prop is true', () => {
    render(
      <ExpandableFilterGroup label="Test label" id="test-id" open>
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - hide options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'true');
  });

  test('clicking button toggles expanded state from closed to open', async () => {
    const { user } = render(
      <ExpandableFilterGroup label="Test label" id="test-id">
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - show options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'false');

    await user.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'true');
    expect(
      screen.getByRole('button', { name: 'Test label - hide options' }),
    ).toBeInTheDocument();
  });

  test('clicking button toggles expanded state from open to closed', async () => {
    const { user } = render(
      <ExpandableFilterGroup label="Test label" id="test-id" open>
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - hide options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'true');

    await user.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'false');
    expect(
      screen.getByRole('button', { name: 'Test label - show options' }),
    ).toBeInTheDocument();
  });

  test('clicking button multiple times toggles state correctly', async () => {
    const { user } = render(
      <ExpandableFilterGroup label="Test label" id="test-id">
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - show options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'false');

    await user.click(button);
    expect(button).toHaveAttribute('aria-expanded', 'true');

    await user.click(button);
    expect(button).toHaveAttribute('aria-expanded', 'false');

    await user.click(button);
    expect(button).toHaveAttribute('aria-expanded', 'true');
  });

  test('calls onToggle when button is clicked with correct arguments', async () => {
    const testOnToggle = jest.fn();

    const { user } = render(
      <ExpandableFilterGroup
        label="Test label"
        id="test-id"
        onToggle={testOnToggle}
      >
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    expect(testOnToggle).not.toHaveBeenCalled();

    await user.click(
      screen.getByRole('button', { name: 'Test label - show options' }),
    );

    expect(testOnToggle).toHaveBeenCalledWith(true, 'test-id');
  });

  test('calls onToggle with false when closing', async () => {
    const testOnToggle = jest.fn();

    const { user } = render(
      <ExpandableFilterGroup
        label="Test label"
        id="test-id"
        open
        onToggle={testOnToggle}
      >
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    await user.click(
      screen.getByRole('button', { name: 'Test label - hide options' }),
    );

    expect(testOnToggle).toHaveBeenCalledWith(false, 'test-id');
  });

  test('updates expanded state when open prop changes', () => {
    const { rerender } = render(
      <ExpandableFilterGroup label="Test label" id="test-id">
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    const button = screen.getByRole('button', {
      name: 'Test label - show options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'false');

    rerender(
      <ExpandableFilterGroup label="Test label" id="test-id" open>
        <p>Test content</p>
      </ExpandableFilterGroup>,
    );

    expect(button).toHaveAttribute('aria-expanded', 'true');
  });
});
