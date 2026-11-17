import render from '@common-test/render';
import { screen } from '@testing-library/react';
import { renderToString } from 'react-dom/server';
import userEvent from '@testing-library/user-event';
import React from 'react';
import Details from '../Details';

describe('Details', () => {
  test('preserves generated IDs when hydrating successive server renders', () => {
    const ui = (
      <>
        <Details summary="First summary">First content</Details>
        <Details summary="Second summary">Second content</Details>
      </>
    );
    const firstMarkup = renderToString(ui);
    expect(renderToString(ui)).toBe(firstMarkup);

    const container = document.createElement('div');
    container.innerHTML = firstMarkup;
    document.body.appendChild(container);
    const firstId = container.querySelector('details')?.id;
    const onRecoverableError = jest.fn();

    render(ui, {
      container,
      hydrate: true,
      onRecoverableError,
      wrapper: ({ children }) => children,
    });

    expect(container.querySelector('details')).toHaveAttribute('id', firstId);
    expect(
      screen.getByRole('button', { name: 'First summary' }),
    ).toHaveAttribute(
      'aria-controls',
      expect.stringContaining('details-content-'),
    );
    expect(onRecoverableError).not.toHaveBeenCalled();
  });

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

  test('clicking the summary reveals the content', async () => {
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

    await userEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByText('Test content')).toBeVisible();
  });

  test('clicking the summary collapses the content when `open` is true', async () => {
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

    await userEvent.click(summary);

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

  test('onToggle handler returns true when expanded', async () => {
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

    await userEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(handleToggle).toHaveBeenCalledWith(true);
  });

  test('onToggle handler returns false when collapsed', async () => {
    const handleToggle = jest.fn();

    render(
      <Details summary="Test summary" id="test-details" onToggle={handleToggle}>
        Test content
      </Details>,
    );

    const summary = screen.getByRole('button', {
      name: 'Test summary',
    });

    await userEvent.click(summary);
    await userEvent.click(summary);

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
