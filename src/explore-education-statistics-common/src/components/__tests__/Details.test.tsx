import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import Details from '../Details';

describe('Details', () => {
  test('renders correctly', () => {
    const { container } = render(
      <Details summary="Test summary" id="test-details">
        Test content
      </Details>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('re-rendering does not change auto-generated `id`', () => {
    const { rerender } = render(
      <Details summary="Test summary">Test content</Details>,
    );

    const originalId = screen
      .getByRole('button')
      .getAttribute('aria-controls') as string;

    expect(originalId.startsWith('details-content-')).toBe(true);

    rerender(<Details summary="Test summary 2">Test content 2</Details>);

    const currentId = screen
      .getByRole('button', {
        name: 'Test summary 2',
      })
      .getAttribute('aria-controls');

    expect(currentId).toBe(originalId);
  });

  test('renders correctly with `open` prop set to true', () => {
    const { container } = render(
      <Details summary="Test summary" id="test-details" open>
        Test content
      </Details>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('open');

    expect(
      screen.getByRole('button', {
        name: 'Test summary',
      }),
    ).toHaveAttribute('aria-expanded', 'true');

    expect(screen.getByText('Test content')).toBeVisible();

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('clicking the summary reveals the content', () => {
    render(
      <Details summary="Test summary" id="test-details">
        Test content
      </Details>,
    );

    const summary = screen.getByRole('button', {
      name: 'Test summary',
    });

    expect(summary).toHaveAttribute('aria-expanded', 'false');
    expect(screen.getByText('Test content')).not.toBeVisible();

    userEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByText('Test content')).toBeVisible();
  });

  test('clicking the summary collapses the content when `open` is true', () => {
    render(
      <Details summary="Test summary" id="test-details" open>
        Test content
      </Details>,
    );

    const summary = screen.getByRole('button', {
      name: 'Test summary',
    });

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByText('Test content')).toBeVisible();

    userEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'false');
    expect(screen.getByText('Test content')).not.toBeVisible();
  });

  test('changing `open` prop from false to true reveals the content', async () => {
    const { rerender } = render(
      <Details summary="Test summary" id="test-details">
        Test content
      </Details>,
    );

    const summary = screen.getByRole('button', {
      name: 'Test summary',
    });

    expect(summary).toHaveAttribute('aria-expanded', 'false');
    expect(screen.getByText('Test content')).not.toBeVisible();

    rerender(
      <Details summary="Test summary" id="test-details" open>
        Test content
      </Details>,
    );

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByText('Test content')).toBeVisible();
  });

  test('changing `open` prop from true to false hides the content', () => {
    const { rerender } = render(
      <Details summary="Test summary" id="test-details" open>
        Test content
      </Details>,
    );

    const summary = screen.getByRole('button', {
      name: 'Test summary',
    });

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByText('Test content')).toBeVisible();

    rerender(
      <Details summary="Test summary" id="test-details">
        Test content
      </Details>,
    );

    expect(summary).toHaveAttribute('aria-expanded', 'false');
    expect(screen.getByText('Test content')).not.toBeVisible();
  });

  test('onToggle handler returns true when expanded', () => {
    const handleToggle = jest.fn();

    render(
      <Details summary="Test summary" id="test-details" onToggle={handleToggle}>
        Test content
      </Details>,
    );

    const summary = screen.getByRole('button', {
      name: 'Test summary',
    });

    expect(summary).toHaveAttribute('aria-expanded', 'false');

    userEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(handleToggle).toHaveBeenCalledWith(true);
  });

  test('onToggle handler returns false when collapsed', () => {
    const handleToggle = jest.fn();

    render(
      <Details summary="Test summary" id="test-details" onToggle={handleToggle}>
        Test content
      </Details>,
    );

    const summary = screen.getByRole('button', {
      name: 'Test summary',
    });

    userEvent.click(summary);
    userEvent.click(summary);

    expect(handleToggle).toHaveBeenNthCalledWith(1, true);
    expect(handleToggle).toHaveBeenNthCalledWith(2, false);
  });

  test('passing a hiddenText prop displays visually hidden text', () => {
    const { container } = render(
      <Details
        summary="Test summary"
        id="test-details"
        hiddenText="publication 1"
      >
        Key stats
      </Details>,
    );

    expect(container.querySelector('.govuk-visually-hidden')).toHaveTextContent(
      'publication 1',
    );
  });
});
