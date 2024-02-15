import Tooltip from '@common/components/Tooltip';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('Tooltip', () => {
  test('renders correctly', async () => {
    render(
      <Tooltip text="Test tooltip" id="test-tooltip">
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    const tooltip = screen.getByRole('tooltip', { hidden: true });

    expect(tooltip).toHaveTextContent('Test tooltip');
    expect(tooltip).toHaveAttribute('id', 'test-tooltip');
  });

  test('does not render if there is no text', async () => {
    render(
      <Tooltip text="" id="test-tooltip">
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  test('does not render if `enabled = false`', async () => {
    render(
      <Tooltip text="Test tooltip" id="test-tooltip" enabled={false}>
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  test('links reference element to tooltip via `aria-describedby`', async () => {
    render(
      <Tooltip text="Test tooltip" id="test-tooltip">
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.getByRole('tooltip', { hidden: true })).toHaveAttribute(
      'id',
      'test-tooltip',
    );
    expect(screen.getByRole('button', { name: 'Test button' })).toHaveAttribute(
      'aria-describedby',
      'test-tooltip',
    );
  });

  test('links reference element with existing `aria-describedby` to tooltip', async () => {
    render(
      <Tooltip text="Test tooltip" id="test-tooltip">
        {({ ref }) => (
          <button aria-describedby="something-else" ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.getByRole('tooltip', { hidden: true })).toHaveAttribute(
      'id',
      'test-tooltip',
    );
    expect(screen.getByRole('button', { name: 'Test button' })).toHaveAttribute(
      'aria-describedby',
      'test-tooltip something-else',
    );
  });

  test('clicking reference element makes tooltip visible', async () => {
    render(
      <Tooltip text="Test tooltip">
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Test button' }));

    expect(screen.getByRole('tooltip')).toBeInTheDocument();
  });

  test('hovering reference element makes tooltip visible', async () => {
    render(
      <Tooltip text="Test tooltip">
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await userEvent.hover(screen.getByRole('button', { name: 'Test button' }));

    expect(screen.getByRole('tooltip')).toBeInTheDocument();
  });

  test('unhovering reference element makes tooltip invisible', async () => {
    render(
      <Tooltip text="Test tooltip">
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    const button = screen.getByRole('button', { name: 'Test button' });

    await userEvent.hover(button);
    await userEvent.unhover(button);

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  test('focusing reference element makes tooltip visible', async () => {
    render(
      <Tooltip text="Test tooltip">
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await userEvent.tab();

    expect(screen.getByRole('button', { name: 'Test button' })).toHaveFocus();
    expect(screen.getByRole('tooltip')).toBeVisible();
  });

  test('blurring reference element makes tooltip invisible', async () => {
    render(
      <Tooltip text="Test tooltip">
        {({ ref }) => (
          <button ref={ref} type="button">
            Test button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await userEvent.tab();
    await userEvent.tab();

    expect(
      screen.getByRole('button', { name: 'Test button' }),
    ).not.toHaveFocus();
    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });

  test('calling `show` render prop makes tooltip visible', async () => {
    render(
      <Tooltip text="Test tooltip">
        {({ show }) => (
          <button type="button" onClick={show}>
            Show button
          </button>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Show button' }));

    expect(screen.getByRole('tooltip')).toBeInTheDocument();
  });

  test('calling `hide` render prop makes tooltip invisible', async () => {
    render(
      <Tooltip text="Test tooltip">
        {({ show, hide }) => (
          <>
            <button type="button" onClick={show}>
              Show button
            </button>
            <button type="button" onClick={hide}>
              Hide button
            </button>
          </>
        )}
      </Tooltip>,
    );

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Show button' }));
    await userEvent.click(screen.getByRole('button', { name: 'Hide button' }));

    expect(screen.queryByRole('tooltip')).not.toBeInTheDocument();
  });
});
