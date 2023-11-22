import Modal from '@common/components/Modal';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('Modal', () => {
  test('clicking the trigger button opens the modal', () => {
    render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Open' }));

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();
  });

  test('clicking the close button closes the modal', () => {
    render(
      <Modal
        showClose
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Close' }));

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();
  });

  test('renders when `open` prop changes from false to true', () => {
    const { rerender } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open={false}
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();

    rerender(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();
  });

  test('does not render when `open` prop changes from true to false', () => {
    const { rerender } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Test modal' }),
    ).toBeInTheDocument();

    rerender(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open={false}
      >
        <p>Content</p>
      </Modal>,
    );

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Test modal' }),
    ).not.toBeInTheDocument();
  });

  test('when opened the `onOpen` handler is called', async () => {
    const onOpen = jest.fn();

    render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        onOpen={onOpen}
        open={false}
      >
        <p>Content</p>
      </Modal>,
    );

    expect(onOpen).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Open' }));

    await waitFor(() => {
      expect(onOpen).toHaveBeenCalled();
    });
  });

  test('closing the modal calls the `onExit` handler', async () => {
    const onExit = jest.fn();

    render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        onExit={onExit}
        open
        showClose
      >
        <p>Content</p>
      </Modal>,
    );

    expect(onExit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Close' }));

    await waitFor(() => {
      expect(onExit).toHaveBeenCalled();
    });
  });

  test('pressing Esc key calls `onExit` handler', async () => {
    const onExit = jest.fn();

    render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        onExit={onExit}
        open
        showClose
      >
        <p>Content</p>
      </Modal>,
    );

    expect(onExit).not.toHaveBeenCalled();

    fireEvent.keyDown(screen.getByRole('dialog'), {
      key: 'Escape',
    });

    await waitFor(() => {
      expect(onExit).toHaveBeenCalled();
    });
  });

  test('when open, root element has aria-hidden=true', () => {
    const { container } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(container).toHaveAttribute('aria-hidden', 'true');
  });

  test('when closed, root element does not have aria-hidden', () => {
    const { container, rerender } = render(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open
      >
        <p>Content</p>
      </Modal>,
    );

    expect(container).toHaveAttribute('aria-hidden', 'true');

    rerender(
      <Modal
        triggerButton={<button type="button">Open</button>}
        title="Test modal"
        open={false}
      >
        <p>Content</p>
      </Modal>,
    );

    expect(container).not.toHaveAttribute('aria-hidden');
  });
});
